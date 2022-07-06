namespace PhotoFrameServer.Data;

public class PhotoFrameSlot
{
    public long Id { get; set; }
    public Guid PhotoId { get; set; } = Guid.Empty;
    public Photo Photo { get; set; } = null!;
    public string PhotoFrameId { get; set; } = null!;
    public PhotoFrame PhotoFrame { get; set; } = null!;
    public DateTime? ViewedDateTime { get; set; }

    public PhotoFrameSlot()
    {
    }
}
