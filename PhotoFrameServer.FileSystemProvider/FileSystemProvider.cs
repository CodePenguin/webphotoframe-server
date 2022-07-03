using PhotoFrameServer.Core;
using Microsoft.Extensions.Logging;

namespace PhotoFrameServer;

public class FileSystemProviderSettings
{
    public string Path { get; set; } = string.Empty;
}

public class FileSystemProviderData
{
    public int TestInt { get; set; } = 0;
    public string TestString { get; set; } = string.Empty;
}

public class FileSystemProvider : IPhotoProvider
{
    private readonly ILogger<FileSystemProvider> _logger;
    private IPhotoProviderContext? _context;
    private FileSystemProviderSettings _settings = new();

    public FileSystemProvider(ILogger<FileSystemProvider> logger)
    {
        _logger = logger;
    }

    public IEnumerable<IPhoto> GetPhotos(int photoLimit)
    {
        throw new NotImplementedException();
    }

    public void Initialize(IPhotoProviderContext context)
    {
        _context = context;
        _settings = context.Settings.Get<FileSystemProviderSettings>();
        _logger.LogDebug("Initialized with path: {Path}", _context?.Settings["Path"]);
        _logger.LogDebug("Initialized with path2: {Path}", _settings.Path);

        var data = context.Data.Get<FileSystemProviderData>();
        _logger.LogDebug("Initialized with Data.TestInt: {Value}", data.TestInt);
        _logger.LogDebug("Initialized with Data.TestString: {Value}", data.TestString);

        data.TestInt = DateTime.Now.Millisecond;
        data.TestString = DateTime.Now.ToString();

        context.Data.Set(data);
    }
}

