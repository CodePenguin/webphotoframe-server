namespace PhotoFrameServer.Data;

public class PhotoFrame
{
    public Guid PhotoFrameId { get; set; }
    public List<Photo> Photos { get; set; }

    public PhotoFrame()
    {
    }
}
