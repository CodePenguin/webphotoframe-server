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


    private void AddPhotosToPhotoFrame(string photoFrameId, IEnumerable<IPhotoMetadata> photosMetadata, IPhotoProvider provider)
    {
        _logger.LogDebug("Adding {Count} photos to {PhotoFrameId}...", photoFrameId);
        foreach (var photoMetadata in photosMetadata)
        {
            var fileContents = provider.GetPhotoContents(photoMetadata).ToArray();
            var dataPhoto = new Photo
            {
                ExternalId = photoMetadata.ExternalId,
                FileContents = fileContents,
                FileExtension = photoMetadata.FileExtension,
                PhotoFrameId = photoFrameId
            };
            _db.Add(dataPhoto);
        }
    }

    private void CheckPhotoFrame(string photoFrameId)
    {
        var photoFrame = _db.PhotoFrames.Find(photoFrameId);
        if (photoFrame is not null)
        {
            return;
        }
        photoFrame = new PhotoFrame
        {
            Id = photoFrameId
        };
        _db.PhotoFrames.Add(photoFrame);
    }

    private void CleanupPhotoFrames(List<PhotoFrameConfiguration> photoFrames)
    {
        var knownPhotoFrameIds = new HashSet<string>(photoFrames.Select(p => p.Id));
        foreach(var photoFrame in _db.PhotoFrames)
        {
            if (!knownPhotoFrameIds.Contains(photoFrame.Id))
            {
                _db.Remove(photoFrame);
            }
        }
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
            foreach (var photoFrame in _settings.PhotoFrames)
            {
                _logger.LogDebug("Processing Photo Frame {PhotoFrameId}...", photoFrame.Id);
                CheckPhotoFrame(photoFrame.Id);

                var providerInstances = new Dictionary<string, PhotoProviderInstanceData>();
                foreach (var provider in photoFrame.Providers)
                {
                    try
                    {
                        if (GetPhotoProviderInstanceData(providerInstances, photoFrame.Id, provider) is not PhotoProviderInstanceData providerData)
                        {
                            continue;
                        }
                        _logger.LogDebug("Requesting photos from {ProviderInstanceId} ({ProviderType})...", provider.Id, providerData.Instance.GetType());
                        var photoLimit = 5;
                        var photos = providerData.Instance.GetPhotos(photoLimit);
                        AddPhotosToPhotoFrame(photoFrame.Id, photos, providerData.Instance);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unhandled exception occurred for {ProviderInstanceId} ({ProviderType})", provider.Id, provider.ProviderType);
                    }
                }
                DeinitializeProviderInstances(providerInstances);
            }
            CleanupPhotoFrames(_settings.PhotoFrames);
            _db.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred while updating photo frames");
        }
        return Task.CompletedTask;
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
