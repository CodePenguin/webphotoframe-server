namespace PhotoFrameServer.Core;

public interface IPhotoProviderContext
{
    public IObjectDictionary Data { get; }
    public IReadOnlyObjectDictionary Settings { get; }
}
