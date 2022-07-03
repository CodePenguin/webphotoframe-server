using PhotoFrameServer.Core;
using Microsoft.Extensions.Logging;

namespace PhotoFrameServer;

public class FileSystemProvider : IPhotoProvider
{
    private readonly ILogger<FileSystemProvider> _logger;
    private IPhotoProviderContext? _context;

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
        _logger.LogDebug("Initialized with path: {Path}", _context?.Settings["Path"]);
    }
}

