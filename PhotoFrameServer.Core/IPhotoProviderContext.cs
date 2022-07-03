namespace PhotoFrameServer.Core;

public interface IPhotoProviderContext
{
    public string Data { get; set; }
    public IReadOnlyDictionary<string, object> Settings { get; }
}
