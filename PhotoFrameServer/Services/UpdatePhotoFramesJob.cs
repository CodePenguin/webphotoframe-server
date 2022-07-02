using Microsoft.Extensions.Options;
using PhotoFrameServer.Configuration;
using Quartz;

namespace PhotoFrameServer.Services;

[DisallowConcurrentExecution]
public class UpdatePhotoFramesJob : IJob
{
    private readonly ILogger<UpdatePhotoFramesJob> _logger;
    private readonly PhotoFramesSettings _settings;

    public UpdatePhotoFramesJob(ILogger<UpdatePhotoFramesJob> logger, IOptionsSnapshot<PhotoFramesSettings> settingsSnapshot)
    {
        _logger = logger;
        _settings = settingsSnapshot.Value;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogDebug("Updating Photo Frames...");

        foreach (var photoFrame in _settings.PhotoFrames)
        {
            _logger.LogDebug("Found Frame {Id}", photoFrame.Id);

            foreach (var provider in photoFrame.Providers)
            {
                _logger.LogDebug("Found Provider {Id} - {ProviderType}", provider.Id, provider.ProviderType);
            }
        }

        return Task.CompletedTask;
    }
}
