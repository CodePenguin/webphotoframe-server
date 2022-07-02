using PhotoFrameServer.Core;

namespace PhotoFrameServer.FileSystemProvider;

public class FileSystemProvider : IPhotoProvider
{
    public IEnumerable<IPhoto> GetPhotos(IPhotoProviderConfiguration configuration, int limit)
    {
        throw new NotImplementedException();
    }
}

