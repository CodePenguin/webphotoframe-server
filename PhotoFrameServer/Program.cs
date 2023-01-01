using PhotoFrameServer.Extensions;
using PhotoFrameServer.Services;
using Serilog;

var applicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PhotoFrameServer");
Directory.CreateDirectory(applicationDataPath);

var command = args.Length > 0 ? args[0].ToLower() : string.Empty;
var isExecutingCommand = CommandService.IsHandledCommand(command);

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory,
    WebRootPath = "wwwroot"
});

// Logging Configuration
builder.Logging.ClearProviders();
var logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(applicationDataPath, "PhotoFrameServer.log"))
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Logging.AddSerilog(logger);

// Service Configuration
builder.AddCommandLineArguments(args);
builder.AddPhotoFrameDbContext(applicationDataPath);
builder.AddPhotoFrameServices(isExecutingCommand);
builder.AddPhotoProviders();

// Initialize application
var app = builder.Build();
app.Logger.LogInformation("Application Data Path: {ApplicationDataPath}", applicationDataPath);

app.UseStaticFiles();
app.UseRouting();
app.MapFallbackToFile("index.html");
app.MapPhotoFrameEndpoints();
app.ExecuteDatabaseMigrations();

try
{
    app.Run();
}
catch (TaskCanceledException)
{
    // Ignore because host application is stopping
}
