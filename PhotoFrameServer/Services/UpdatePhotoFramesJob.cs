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


    private void AddPhotosToPhotoFrame(string photoFrameId, IEnumerable<IPhoto> photos)
    {
        _logger.LogDebug("Adding {Count} photos to {PhotoFrameId}...", photoFrameId);
        foreach (var photo in photos)
        {
            var dbPhoto = new Photo
            {
                PhotoFrameId = photoFrameId,
                FileContents = photo.FileContents,
                FileExtension = photo.FileExtension
            };
            _db.Add(dbPhoto);
        }
    }


    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogDebug("Updating Photo Frames...");

        foreach (var photoFrame in _settings.PhotoFrames)
        {
            _logger.LogDebug("Processing Photo Frame {PhotoFrameId}...", photoFrame.Id);

            var providerInstances = new Dictionary<string, PhotoProviderInstanceData>();
            foreach (var provider in photoFrame.Providers)
            {
                if (GetPhotoProviderInstanceData(providerInstances, photoFrame.Id, provider) is not PhotoProviderInstanceData providerData)
                {
                    continue;
                }
                _logger.LogDebug("Requesting photos from {ProviderInstanceId} ({ProviderType})...", provider.Id, providerData.Instance.GetType());
                var photoLimit = 5;
                var photos = providerData.Instance.GetPhotos(photoLimit);
                AddPhotosToPhotoFrame(photoFrame.Id, photos);
            }
            SetPhotoProviderInstanceData(providerInstances);
        }
        _db.SaveChanges();
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

    private void SetPhotoProviderInstanceData(Dictionary<string, PhotoProviderInstanceData> providerInstances)
    {
        foreach(var (providerInstanceId, providerInstance) in providerInstances)
        {
            if (!providerInstance.Context.Modified)
            {
                continue;
            }
            _db.SetPhotoProviderInstanceData(providerInstance.PhotoFrameId, providerInstanceId, providerInstance.Context.Data);
        }
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
