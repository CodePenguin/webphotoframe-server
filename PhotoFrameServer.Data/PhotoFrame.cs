namespace PhotoFrameServer.Data;

public class PhotoFrame
{
    public string PhotoFrameId { get; set; } = null!;
    public List<Photo> Photos { get; set; } = new();

    public PhotoFrame()
    {
    }
}
