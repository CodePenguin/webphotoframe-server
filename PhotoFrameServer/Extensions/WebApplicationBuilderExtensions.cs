using Microsoft.EntityFrameworkCore;
using PhotoFrameServer.Data;

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
}

