using Microsoft.Extensions.Options;
using PhotoFrameServer.Configuration;
using PhotoFrameServer.Core;
using Quartz;

namespace PhotoFrameServer.Services;

[DisallowConcurrentExecution]
public class UpdatePhotoFramesJob : IJob
{
    private readonly ILogger<UpdatePhotoFramesJob> _logger;
    private readonly PhotoFramesSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private readonly PhotoProviderService _photoProviderService;

    public UpdatePhotoFramesJob(
        ILogger<UpdatePhotoFramesJob> logger,
        IOptionsSnapshot<PhotoFramesSettings> settingsSnapshot,
        IServiceProvider serviceProvider,
        PhotoProviderService photoProviderService)
    {
        _logger = logger;
        _settings = settingsSnapshot.Value;
        _serviceProvider = serviceProvider;
        _photoProviderService = photoProviderService;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogDebug("Updating Photo Frames...");

        foreach (var photoFrame in _settings.PhotoFrames)
        {
            _logger.LogDebug("Processing Photo Frame {Id}...", photoFrame.Id);

            var providerInstances = new Dictionary<string, IPhotoProvider>();
            foreach (var provider in photoFrame.Providers)
            {
                if (GetPhotoProviderInstance(providerInstances, provider) is not IPhotoProvider providerInstance)
                {
                    continue;
                }
                _logger.LogDebug("Requesting photos from {Id} ({ProviderType})...", photoFrame.Id, providerInstance);
            }
        }

        return Task.CompletedTask;
    }

    private IPhotoProvider? GetPhotoProviderInstance(Dictionary<string, IPhotoProvider> providerInstances, PhotoProviderConfiguration provider)
    {
        if (providerInstances.TryGetValue(provider.Id, out var providerInstance))
        {
            return providerInstance;
        }

        _logger.LogDebug("Initializing new provider instance {ProviderId} ({ProviderType})...", provider.Id, provider.ProviderType);
        var providerType = _photoProviderService.FindProviderType(provider.ProviderType);
        if (providerType is null)
        {
            _logger.LogError("Invalid provider type: {ProviderType}", provider.ProviderType);
            return null;
        }

        providerInstance = _serviceProvider.GetService(providerType) as IPhotoProvider;
        if (providerInstance is null)
        {
            _logger.LogError("Unable to create provider instance: {ProviderType}", provider.ProviderType);
            return null;
        }
        _logger.LogDebug("Initialized provider instance {ProviderId} ({ProviderType}).", provider.Id, providerInstance);

        var photoProviderSettings = new PhotoProviderContext
        {
            Settings = new ObjectDictionary(provider.Settings)
        };

        providerInstance.Initialize(photoProviderSettings);
        providerInstances.Add(provider.Id, providerInstance);
        return providerInstance;
    }
}
