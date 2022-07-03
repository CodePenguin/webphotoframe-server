namespace PhotoFrameServer.Data;

public class PhotoFrame
{
    public string Id { get; set; } = null!;
    public List<Photo> Photos { get; set; } = new();
    public List<PhotoProviderInstance> PhotoProviderInstances { get; set; } = new();

    public PhotoFrame()
    {
    }
}
