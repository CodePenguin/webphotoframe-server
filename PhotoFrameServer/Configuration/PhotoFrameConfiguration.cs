namespace PhotoFrameServer.Configuration;

public class PhotoFrameConfiguration
{
    public bool Enabled { get; set; } = true;
    public int? ConfigRefreshIntervalSeconds { get; set; } = null;
    public int? ExpirePhotoAfterFirstViewSeconds { get; set; } = null;
    public int? ExpirePhotoAfterViewedCount { get; set; } = null;
    public string Id { get; set; } = string.Empty;
    public int? MaxPhotoFrameSlotCount { get; set; } = null;
    public int? PhotoSwitchIntervalSeconds { get; set; } = null;
    public List<PhotoProviderConfiguration> Providers { get; set; } = new();

    public PhotoFrameConfiguration()
    {
    }
}
