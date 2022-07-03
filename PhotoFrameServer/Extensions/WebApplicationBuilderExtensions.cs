using Microsoft.EntityFrameworkCore;
using PhotoFrameServer.Configuration;
using PhotoFrameServer.Data;
using PhotoFrameServer.Services;
using Quartz;

namespace PhotoFrameServer.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void AddPhotoFrameDbContext(this WebApplicationBuilder builder, string applicationDataPath)
    {
        builder.Services.AddDbContext<PhotoFrameContext>(options => {
            var databaseFilename = Path.Combine(applicationDataPath, "PhotoFrameServer.db");
            options.UseSqlite($"Data Source={databaseFilename}", builder => builder.MigrationsAssembly("PhotoFrameServer"));
        });
    }

    public static void AddPhotoFrameServices(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<PhotoFramesSettings>(builder.Configuration.GetSection(PhotoFramesSettings.Key));

        builder.Services.AddScoped<PhotoFrameRequestHandler>();

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

    public static void AddPhotoProviders(this WebApplicationBuilder builder)
    {
        var photoProviderService = new PhotoProviderService();
        builder.Services.AddSingleton(photoProviderService);

        builder.Services.AddScoped(typeof(FileSystemProvider));
        photoProviderService.RegisterProviderType(typeof(FileSystemProvider));
    }
}
