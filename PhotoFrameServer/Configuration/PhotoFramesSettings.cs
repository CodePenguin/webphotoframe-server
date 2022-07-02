namespace PhotoFrameServer.Configuration;

public class PhotoFramesSettings
{
    public const string Key = "Settings";

    public List<PhotoFrameConfiguration> PhotoFrames { get; set; } = new();

    public PhotoFramesSettings()
    {
    }
}
