using Microsoft.EntityFrameworkCore;
using PhotoFrameServer.Data;

namespace PhotoFrameServer.Extensions;

public static class WebApplicationExtensions
{
    public static void ExecuteDatabaseMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dataContext = scope.ServiceProvider.GetRequiredService<PhotoFrameContext>();
        dataContext.Database.Migrate();
    }
}
