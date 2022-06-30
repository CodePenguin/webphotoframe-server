namespace PhotoFrameServer.Data;

public class Photo
{
    public Guid PhotoId { get; set; }
    public string Caption { get; set; }
    public byte[] FileContents { get; set; }
    public string FileExtension { get; set; }

    public Guid PhotoFrameId { get; set; }
    public PhotoFrame PhotoFrame { get; set; }

    public Photo()
    {
    }
}
