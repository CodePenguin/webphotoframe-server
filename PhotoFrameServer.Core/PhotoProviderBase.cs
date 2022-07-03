using Microsoft.Extensions.Logging;

namespace PhotoFrameServer.Core;

public abstract class PhotoProviderBase : IPhotoProvider
{
    private IPhotoProviderContext? _context = null;
    protected readonly ILogger Logger;
    protected IPhotoProviderContext Context => _context ?? throw new ArgumentException($"{GetType()}.{nameof(Initialize)} was not called.");

    public PhotoProviderBase(ILogger logger)
    {
        Logger = logger;
    }

    public abstract IEnumerable<IPhoto> GetPhotos(int photoLimit);

    public virtual void Initialize(IPhotoProviderContext context)
    {
        _context = context;
    }
}