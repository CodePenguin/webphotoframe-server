namespace PhotoFrameServer.Configuration;

public class PhotoFrameConfiguration
{
    public string Id { get; set; } = string.Empty;
    public int ConfigRefreshIntervalSeconds { get; set; } = 60;
    public int PhotoSwitchIntervalSeconds { get; set; } = 30;
    public List<PhotoProviderConfiguration> Providers { get; set; } = new();

    public PhotoFrameConfiguration()
    {
    }
}
