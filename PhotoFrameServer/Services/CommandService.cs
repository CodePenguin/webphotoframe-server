using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PhotoFrameServer.Configuration;
using PhotoFrameServer.Core;
using PhotoFrameServer.Data;
using Quartz.Impl.AdoJobStore.Common;

namespace PhotoFrameServer.Services;

public class CommandService : BackgroundService
{
    private readonly CommandLine _commandLine;
    private readonly ILogger<CommandService> _logger;
    private readonly IHostApplicationLifetime _host;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly PhotoFramesSettings _settings;

    public CommandService(
        ILogger<CommandService> logger,
        IHostApplicationLifetime host,
        CommandLine commandLine,
        IServiceScopeFactory scopeFactory,
        IOptions<PhotoFramesSettings> settings)
    {
        _logger = logger;
        _host = host;
        _commandLine = commandLine;
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            HandleCommand();
        }
        catch (CommandException ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            _host.StopApplication();
        }
        return Task.CompletedTask;
    }

    private void HandleCommand()
    {
        var command = _commandLine.Args[0].ToLower();
        var args = _commandLine.Args[1..];
        switch (command)
        {
            case "configure":
                HandleConfigureCommand(args);
                break;
            case "list":
                HandleListCommand(args);
                break;
            default:
                throw new CommandException($"Invalid command: {command}");
        }
    }

    private void HandleConfigureCommand(string[] args)
    {
        var entityType = args.Length > 0 ? args[0].ToLower() : "{Not Provided}";
        switch (entityType)
        {
            case "provider":
                HandleConfigureProviderCommand(args[1..]);
                break;
            default:
                throw new CommandException($"Invalid entity type: {entityType}");
        }
    }

    private void HandleConfigureProviderCommand(string[] args)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var photoFrameProviderId = args.Length > 0 ? args[0] : string.Empty;
            ParsePhotoFrameProviderId(photoFrameProviderId, out var photoFrame, out var provider);
            _logger.LogDebug("Initializing provider instance \"{ProviderId}\" of type \"{ProviderType})\"...", provider.Id, provider.ProviderType);
            var photoProviderService = scope.ServiceProvider.GetRequiredService<PhotoProviderService>();
            var providerType = photoProviderService.GetPhotoProviderType(provider.ProviderType);
            if (providerType is null || scope.ServiceProvider.GetService(providerType) is not IPhotoProvider providerInstance || providerInstance is null)
            {
                throw new CommandException($"Unable to initialize provider instance: {provider.ProviderType}");
            }

            var db = scope.ServiceProvider.GetRequiredService<PhotoFrameDbContext>();
            var data = db.GetPhotoProviderInstanceData(photoFrame.Id, provider.Id);
            var context = new PhotoProviderContext(data, provider.Settings);

            providerInstance.Initialize(context);
            _logger.LogDebug("Configuring provider \"{ProviderId}\" in Photo Frame \"{PhotoFrameId}\"...", provider.Id, photoFrame.Id);
            providerInstance.Configure(args[1..]);
            _logger.LogDebug("Deinitializing provider \"{ProviderId}\"...", provider.Id);
            providerInstance.Deinitialize(context);
            if (context.Modified)
            {
                Console.WriteLine("Configuration changes were saved.");
                db.SetPhotoProviderInstanceData(photoFrame.Id, provider.Id, context.Data);
            }
            else
            {
                Console.WriteLine("No configuration changes were made.");
            }
        }
    }

    private void HandleListCommand(string[] args)
    {
        if (args.Length < 1)
        {
            throw new CommandException("Entity type was not provided");
        }
        var entityType = args[0].ToLower();
        switch (entityType)
        {
            case "provider":
                HandleListProvidersCommand(args[1..]);
                break;
            default:
                throw new CommandException($"Invalid entity type: {entityType}");
        }
    }

    private void HandleListProvidersCommand(string[] args)
    {
        if (args.Length > 0)
        {
            throw new CommandException("List providers does not take any arguments");
        }
        var printedHeader = false;
        foreach (var photoFrameConfiguration in _settings.PhotoFrames.Where(p => p.Enabled))
        {
            foreach (var provider in photoFrameConfiguration.Providers.Where(p => p.Enabled))
            {
                if (!printedHeader)
                {
                    Console.WriteLine("{0,-40} {1,-39}", "Photo Frame Provider ID", "Provider Type");
                    printedHeader = true;
                }
                var photoFrameProviderId = $"{photoFrameConfiguration.Id}:{provider.Id}";
                Console.WriteLine("> {0,-38} {1,-39}", photoFrameProviderId, provider.ProviderType);
            }
        }
        if (!printedHeader)
        {
            Console.WriteLine("No enabled providers were found");
        }
    }

    public static bool IsHandledCommand(string command)
    {
        return (new string[] { "configure", "list" }).Contains(command.ToLower());
    }

    private void ParsePhotoFrameProviderId(string photoFrameProviderId, out PhotoFrameConfiguration photoFrameConfiguration, out PhotoProviderConfiguration photoProviderConfiguration)
    {
        if (string.IsNullOrWhiteSpace(photoFrameProviderId))
        {
            throw new CommandException("Photo Frame Provider ID was not provided");
        }
        var splitIndex = photoFrameProviderId.IndexOf(':');
        var photoFrameId = splitIndex > -1 ? photoFrameProviderId[..splitIndex] : _settings.PhotoFrames[0].Id;
        var providerId = splitIndex > -1 ? photoFrameProviderId[(splitIndex + 1)..] : photoFrameProviderId;

        photoFrameConfiguration = _settings.PhotoFrames.FirstOrDefault(p => p.Id == photoFrameId)!;
        if (photoFrameConfiguration is null)
        {
            throw new CommandException($"Photo Frame not found: {photoFrameId}");
        }
        photoProviderConfiguration = photoFrameConfiguration.Providers.FirstOrDefault(p => p.Id == providerId)!;
        if (photoProviderConfiguration is null)
        {
            throw new CommandException($"Provider not found: {providerId}");
        }
    }
}
