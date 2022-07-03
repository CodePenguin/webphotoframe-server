namespace PhotoFrameServer.Core;

public interface IPhotoProvider
{
    public void Initialize(IPhotoProviderContext context);
    public IEnumerable<IPhoto> GetPhotos(int photoLimit);
}
