using Microsoft.Extensions.Options;
using PhotoFrameServer.Configuration;
using PhotoFrameServer.Core;
using PhotoFrameServer.Data;
using Quartz;

namespace PhotoFrameServer.Services;

[DisallowConcurrentExecution]
public class UpdatePhotoFramesJob : IJob
{
    private readonly ILogger<UpdatePhotoFramesJob> _logger;
    private readonly PhotoFramesSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private readonly PhotoProviderService _photoProviderService;
    private readonly PhotoFrameDbContext _db;

    public UpdatePhotoFramesJob(
        ILogger<UpdatePhotoFramesJob> logger,
        IOptionsSnapshot<PhotoFramesSettings> settingsSnapshot,
        IServiceProvider serviceProvider,
        PhotoProviderService photoProviderService,
        PhotoFrameDbContext db)
    {
        _logger = logger;
        _settings = settingsSnapshot.Value;
        _serviceProvider = serviceProvider;
        _photoProviderService = photoProviderService;
        _db = db;
    }

    private void AddPhotosToPhotoFrame(PhotoFrame photoFrame, IEnumerable<IPhotoMetadata> photosMetadata, IPhotoProvider provider)
    {
        var addedCount = 0;
        var totalCount = 0;
        foreach (var photoMetadata in photosMetadata)
        {
            totalCount++;
            var photo = _db.GetPhotoByExternalId(photoMetadata.ExternalId);
            if (photo is not null && photoFrame.Slots.Any(p => p.PhotoId == photo.Id))
            {
                continue;
            }
            if (photo is null)
            {
                var fileContents = provider.GetPhotoContents(photoMetadata).ToArray();
                photo = new Photo
                {
                    ExternalId = photoMetadata.ExternalId,
                    FileContents = fileContents,
                    FileExtension = photoMetadata.FileExtension
                };
                _db.Add(photo);
            }
            var slot = new PhotoFrameSlot
            {
                PhotoFrame = photoFrame,
                Photo = photo
            };
            photoFrame.Slots.Add(slot);
            addedCount++;

            // Mark an expired slot as replaced if available
            var slotToReplace = photoFrame.Slots.Where(s => s.ExpiredDateTime is not null && s.ReplacedDateTime is null).FirstOrDefault();
            if (slotToReplace is not null)
            {
                slotToReplace.ReplacedDateTime = DateTime.Now;
            }
        }
        _logger.LogDebug("Added {AddedCount} of {TotalCount} photos to {PhotoFrameId}", addedCount, totalCount, photoFrame.Id);
    }

    private void CleanupUnusedPhotoFrameData()
    {
        var knownPhotoFrameIds = new HashSet<string>(_settings.PhotoFrames.Select(p => p.Id));
        var unknownPhotoFrames = _db.PhotoFrames.Where(p => !knownPhotoFrameIds.Contains(p.Id));
        foreach (var photoFrame in unknownPhotoFrames)
        {
            _logger.LogInformation("Removed data for unused photo frame {PhotoFrameId}.", photoFrame.Id);
        }
        _db.PhotoFrames.RemoveRange(unknownPhotoFrames);
    }

    private void CleanupReplacedPhotoFrameSlots(PhotoFrameConfiguration photoFrameConfiguration, PhotoFrame photoFrame)
    {
        var configRefreshIntervalSeconds = photoFrameConfiguration.ConfigRefreshIntervalSeconds ?? _settings.DefaultConfigRefreshIntervalSeconds;
        var replacedSlots = photoFrame.Slots.Where(s => s.ReplacedDateTime?.AddSeconds(configRefreshIntervalSeconds) < DateTime.Now);
        _db.PhotoFrameSlots.RemoveRange(replacedSlots);
    }

    private void DeinitializeProviderInstances(Dictionary<string, PhotoProviderInstanceData> providerInstances)
    {
        foreach (var providerInstanceId in providerInstances.Keys)
        {
            var data = providerInstances[providerInstanceId];
            providerInstances.Remove(providerInstanceId);
            data.Instance.Deinitialize(data.Context);
            if (data.Context.Modified)
            {
                _db.SetPhotoProviderInstanceData(data.PhotoFrameId, providerInstanceId, data.Context.Data);
            }
        }
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogDebug("Updating Photo Frames...");
        try
        {
            CleanupUnusedPhotoFrameData();
            foreach (var photoFrameConfiguration in _settings.PhotoFrames)
            {
                ProcessPhotoFrame(photoFrameConfiguration);
            }
            _db.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred while updating photo frames");
        }
        return Task.CompletedTask;
    }

    private void ProcessPhotoFrame(PhotoFrameConfiguration photoFrameConfiguration)
    {
        _logger.LogDebug("Processing Photo Frame {PhotoFrameId}...", photoFrameConfiguration.Id);

        var maxSlotCount = photoFrameConfiguration.MaxPhotoFrameSlotCount ?? _settings.DefaultMaxPhotoFrameSlotCount;
        var photoFrame = _db.AddOrGetPhotoFrame(photoFrameConfiguration.Id);

        var expiredCount = MarkAndCountExpiredPhotoFrameSlots(photoFrameConfiguration, photoFrame);

        var providerInstances = new Dictionary<string, PhotoProviderInstanceData>();
        foreach (var provider in photoFrameConfiguration.Providers)
        {
            var photoLimit = Math.Max(0, maxSlotCount - (photoFrame.Slots.Count - expiredCount));
            if (photoLimit <= 0)
            {
                break;
            }
            try
            {
                if (GetPhotoProviderInstanceData(providerInstances, photoFrameConfiguration.Id, provider) is not PhotoProviderInstanceData providerData)
                {
                    continue;
                }
                _logger.LogDebug("Requesting photos from {ProviderInstanceId} ({ProviderType})...", provider.Id, providerData.Instance.GetType());
                var photos = providerData.Instance.GetPhotos(photoLimit);
                AddPhotosToPhotoFrame(photoFrame, photos, providerData.Instance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred for {ProviderInstanceId} ({ProviderType})", provider.Id, provider.ProviderType);
            }
        }
        DeinitializeProviderInstances(providerInstances);
        CleanupReplacedPhotoFrameSlots(photoFrameConfiguration, photoFrame);
    }

    private PhotoProviderInstanceData? GetPhotoProviderInstanceData(Dictionary<string, PhotoProviderInstanceData> providerInstances, string photoFrameId, PhotoProviderConfiguration provider)
    {
        if (providerInstances.TryGetValue(provider.Id, out var providerInstanceData))
        {
            return providerInstanceData;
        }

        _logger.LogDebug("Initializing new provider instance {ProviderId} ({ProviderType})...", provider.Id, provider.ProviderType);
        var providerType = _photoProviderService.FindProviderType(provider.ProviderType);
        if (providerType is null)
        {
            _logger.LogError("Invalid provider type: {ProviderType}", provider.ProviderType);
            return null;
        }

        if (_serviceProvider.GetService(providerType) is not IPhotoProvider providerInstance)
        {
            _logger.LogError("Unable to create provider instance: {ProviderType}", provider.ProviderType);
            return null;
        }
        _logger.LogDebug("Initialized provider instance {ProviderId} ({ProviderType}).", provider.Id, providerInstance);

        var data = _db.GetPhotoProviderInstanceData(photoFrameId, provider.Id);
        var context = new PhotoProviderContext(data, provider.Settings);

        providerInstanceData = new PhotoProviderInstanceData(photoFrameId, providerInstance, context);
        providerInstances.Add(provider.Id, providerInstanceData);

        providerInstance.Initialize(context);
        return providerInstanceData;
    }

    private int MarkAndCountExpiredPhotoFrameSlots(PhotoFrameConfiguration photoFrameConfiguration, PhotoFrame photoFrame)
    {
        var expirePhotoAfterFirstViewSeconds = photoFrameConfiguration.ExpirePhotoAfterFirstViewSeconds ?? _settings.DefaultExpirePhotoAfterFirstViewSeconds;
        var expirePhotoAfterViewedCount = photoFrameConfiguration.ExpirePhotoAfterViewedCount ?? _settings.DefaultExpirePhotoAfterViewedCount;
        var expiredCount = 0;
        var nowDateTime = DateTime.Now;
        foreach (var slot in photoFrame.Slots)
        {
            if (slot.ExpiredDateTime is not null && slot.ReplacedDateTime is not null)
            {
                expiredCount++;
                continue;
            }

            var expiredByViewedDateTime = expirePhotoAfterFirstViewSeconds > 0 && slot.ViewedDateTime?.AddSeconds(expirePhotoAfterFirstViewSeconds) < nowDateTime;
            var expiredByViewCount = expirePhotoAfterViewedCount > 0 && slot.ViewedCount >= expirePhotoAfterViewedCount;

            if (expiredByViewedDateTime || expiredByViewCount)
            {
                slot.ExpiredDateTime = nowDateTime;
                expiredCount++;
            }
        }
        return expiredCount;
    }

    private class PhotoProviderInstanceData
    {
        public string PhotoFrameId { get; set; }
        public IPhotoProvider Instance { get; private set; }
        public PhotoProviderContext Context { get; private set; }

        public PhotoProviderInstanceData(string photoFrameId, IPhotoProvider instance, PhotoProviderContext context)
        {
            PhotoFrameId = photoFrameId;
            Instance = instance;
            Context = context;
        }
    }
}
