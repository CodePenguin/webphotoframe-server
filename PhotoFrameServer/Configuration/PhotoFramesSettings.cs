namespace PhotoFrameServer.Configuration;

public class PhotoFramesSettings
{
    public const string Key = "Settings";

    public int DefaultConfigRefreshIntervalSeconds { get; set; } = 300;
    public int DefaultExpirePhotoAfterFirstViewSeconds { get; set; } = 300;
    public int DefaultExpirePhotoAfterViewedCount { get; set; } = 0;
    public int DefaultMaxPhotoFrameSlotCount { get; set; } = 50;
    public int DefaultPhotoSwitchIntervalSeconds { get; set; } = 30;
    public List<PhotoFrameConfiguration> PhotoFrames { get; set; } = new();

    public PhotoFramesSettings()
    {
    }
}
