using PhotoFrameServer.Extensions;
using Serilog;

var applicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PhotoFrameServer");
Directory.CreateDirectory(applicationDataPath);

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory,
    WebRootPath = "wwwroot"
});

// Logging Configuration
builder.Logging.ClearProviders();
var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(applicationDataPath, "PhotoFrameServer.log"))
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Logging.AddSerilog(logger);

builder.AddPhotoFrameDbContext(applicationDataPath);

var app = builder.Build();
app.Logger.LogInformation("Application Data Path: {ApplicationDataPath}", applicationDataPath);

app.UseStaticFiles();
app.UseRouting();
app.MapFallbackToFile("index.html");
app.MapPhotoFrameEndpoints();
app.ExecuteDatabaseMigrations();
app.Run();

