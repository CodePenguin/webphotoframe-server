using Microsoft.EntityFrameworkCore;
using PhotoFrameServer.Configuration;
using PhotoFrameServer.Core;
using PhotoFrameServer.Data;
using PhotoFrameServer.Services;
using Quartz;

namespace PhotoFrameServer.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void AddCommandLineArguments(this WebApplicationBuilder builder, string[] args)
    {
        builder.Services.AddSingleton(new CommandLine { Args = args });
    }

    public static void AddPhotoFrameDbContext(this WebApplicationBuilder builder, string applicationDataPath)
    {
        builder.Services.AddDbContext<PhotoFrameDbContext>(options => {
            var databaseFilename = Path.Combine(applicationDataPath, "PhotoFrameServer.db");
            options.UseSqlite($"Data Source={databaseFilename}", builder => builder.MigrationsAssembly("PhotoFrameServer"));
        });
    }

    public static void AddPhotoFrameServices(this WebApplicationBuilder builder, bool isExecutingCommand)
    {
        builder.Services.Configure<PhotoFramesSettings>(builder.Configuration.GetSection(PhotoFramesSettings.Key));
        builder.Services.AddScoped<PhotoFrameRequestHandler>();

        if (isExecutingCommand)
        {
            builder.Services.AddHostedService<CommandService>();
        }
        else
        {
            builder.Services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();

                var jobKey = new JobKey(nameof(UpdatePhotoFramesJob));

                q.AddJob<UpdatePhotoFramesJob>(options => options
                    .WithIdentity(jobKey));

                q.AddTrigger(options => options
                    .ForJob(jobKey)
                    .StartNow()
                    .WithSimpleSchedule(schedule => schedule
                        .WithIntervalInSeconds(10)
                        .RepeatForever()));
            });

            builder.Services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });
        }
    }

    public static void AddPhotoProviders(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<PhotoProviderService>();

        var knownProviderTypes = new KnownPhotoProviderTypes();
        knownProviderTypes.Add(typeof(FileSystemProvider));
        // TODO: Add providers from plugins to the list

        // Register the list of known providers
        builder.Services.AddTransient<KnownPhotoProviderTypes>(p => knownProviderTypes);

        // Register each individual IPhotoProvider type
        foreach (var providerType in knownProviderTypes)
        {
            builder.Services.AddScoped(providerType);
        }
    }
}
