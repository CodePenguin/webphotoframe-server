namespace PhotoFrameServer.Configuration;

public class PhotoFrameConfiguration
{
    public string Id { get; set; } = string.Empty;
    public int? ConfigRefreshIntervalSeconds { get; set; } = null;
    public int? PhotoSwitchIntervalSeconds { get; set; } = null;
    public List<PhotoProviderConfiguration> Providers { get; set; } = new();

    public PhotoFrameConfiguration()
    {
    }
}
