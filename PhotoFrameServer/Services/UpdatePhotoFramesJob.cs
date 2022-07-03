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
    private readonly PhotoProviderInstanceService _photoProviderInstanceService;

    public UpdatePhotoFramesJob(
        ILogger<UpdatePhotoFramesJob> logger,
        IOptionsSnapshot<PhotoFramesSettings> settingsSnapshot,
        IServiceProvider serviceProvider,
        PhotoProviderService photoProviderService,
        PhotoProviderInstanceService photoProviderInstanceService)
    {
        _logger = logger;
        _settings = settingsSnapshot.Value;
        _serviceProvider = serviceProvider;
        _photoProviderService = photoProviderService;
        _photoProviderInstanceService = photoProviderInstanceService;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogDebug("Updating Photo Frames...");

        foreach (var photoFrame in _settings.PhotoFrames)
        {
            _logger.LogDebug("Processing Photo Frame {Id}...", photoFrame.Id);

            var providerInstances = new Dictionary<string, PhotoProviderInstanceData>();
            foreach (var provider in photoFrame.Providers)
            {
                if (GetPhotoProviderInstanceData(providerInstances, photoFrame.Id, provider) is not PhotoProviderInstanceData providerData)
                {
                    continue;
                }
                _logger.LogDebug("Requesting photos from {Id} ({ProviderType})...", photoFrame.Id, providerData.Instance);
            }
            SavePhotoProviderInstanceData(providerInstances);
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

        var context = new PhotoProviderContext
        {
            Data = new ObjectDictionary(_photoProviderInstanceService.GetData(photoFrameId, provider.Id)),
            Settings = new ObjectDictionary(provider.Settings)
        };

        providerInstanceData = new PhotoProviderInstanceData(photoFrameId, provider.Id, providerInstance, context);
        providerInstances.Add(provider.Id, providerInstanceData);

        providerInstance.Initialize(context);
        return providerInstanceData;
    }

    private void SavePhotoProviderInstanceData(Dictionary<string, PhotoProviderInstanceData> providerInstances)
    {
        foreach(var (key, providerInstance) in providerInstances)
        {
            if (!providerInstance.Context.Data.Modified)
            {
                continue;
            }
            _logger.LogDebug("Data changed {ProviderId} ({ProviderType}).", key, providerInstance);
            _photoProviderInstanceService.SaveData(providerInstance.PhotoFrameId, providerInstance.PhotoProviderInstanceId, providerInstance.Context.Data);
        }
        _photoProviderInstanceService.SaveAllChanges();
    }

    private class PhotoProviderInstanceData
    {
        public string PhotoFrameId { get; set; }
        public string PhotoProviderInstanceId { get; set; }
        public IPhotoProvider Instance { get; private set; }
        public PhotoProviderContext Context { get; private set; }

        public PhotoProviderInstanceData(string photoFrameId, string photoProviderInstanceId, IPhotoProvider instance, PhotoProviderContext context)
        {
            PhotoFrameId = photoFrameId;
            PhotoProviderInstanceId = photoProviderInstanceId;
            Instance = instance;
            Context = context;
        }
    }
}
