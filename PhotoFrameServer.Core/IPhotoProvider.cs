namespace PhotoFrameServer.Core;

public interface IPhotoProvider
{
    public void Initialize(IPhotoProviderContext context);
    public void Deinitialize(IPhotoProviderContext context);

    public IEnumerable<IPhotoMetadata> GetPhotos(int photoLimit);
    public IEnumerable<byte>GetPhotoContents(IPhotoMetadata photoMetadata);
}
