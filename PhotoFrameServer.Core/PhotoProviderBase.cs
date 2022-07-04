using System.Security.Cryptography;
using System.Text;
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

    public virtual void Initialize(IPhotoProviderContext context)
    {
        _context = context;
    }

    public virtual void Deinitialize(IPhotoProviderContext context)
    {
        _context = null;
    }

    protected string GenerateExternalId(string data)
    {
        var dataBytes = Encoding.UTF8.GetBytes($"{GetType().FullName}:{data}");
        using var sha256 = SHA1.Create();
        return Convert.ToBase64String(sha256.ComputeHash(dataBytes));
    }

    public abstract IEnumerable<IPhotoMetadata> GetPhotos(int photoLimit);
    public abstract IEnumerable<byte> GetPhotoContents(IPhotoMetadata photoMetaData);
}