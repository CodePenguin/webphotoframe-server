namespace PhotoFrameServer.Data;

public class PhotoFrame
{
    public string Id { get; set; } = null!;
    public List<PhotoFrameSlot> Slots { get; set; } = new();
    public List<PhotoProviderInstance> PhotoProviderInstances { get; set; } = new();

    public PhotoFrame()
    {
    }
}
