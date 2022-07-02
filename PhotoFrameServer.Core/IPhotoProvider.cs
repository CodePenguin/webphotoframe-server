namespace PhotoFrameServer.Core;

public interface IPhotoProvider
{
    public IEnumerable<IPhoto> GetPhotos(IPhotoProviderConfiguration configuration, int limit);
}
