namespace PhotoFrameServer.Data;

public class PhotoFrameSlot
{
    public DateTime? ExpiredDateTime { get; set; }
    public long Id { get; set; }
    public Guid PhotoId { get; set; } = Guid.Empty;
    public Photo Photo { get; set; } = null!;
    public string PhotoFrameId { get; set; } = null!;
    public PhotoFrame PhotoFrame { get; set; } = null!;
    public DateTime? ReplacedDateTime { get; set; }
    public DateTime? ViewedDateTime { get; set; }
    public int ViewedCount { get; set; }

    public PhotoFrameSlot()
    {
    }
}
