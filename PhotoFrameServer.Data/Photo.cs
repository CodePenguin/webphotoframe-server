namespace PhotoFrameServer.Data;

public class Photo
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Caption { get; set; } = string.Empty;
    public string ExternalId { get; set; } = null!;
    public byte[] FileContents { get; set; } = null!;
    public string FileExtension { get; set; } = null!;
    public string PhotoFrameId { get; set; } = null!;
    public PhotoFrame PhotoFrame { get; set; } = null!;
    public DateTime? ViewedDateTime { get; set; }

    public Photo()
    {
    }
}
