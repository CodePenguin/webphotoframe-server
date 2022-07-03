namespace PhotoFrameServer.Data;

public class PhotoProviderInstance
{
    public string Id { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public string PhotoFrameId { get; set; } = null!;
    public PhotoFrame PhotoFrame { get; set; } = null!;

    public PhotoProviderInstance()
    {
    }
}
