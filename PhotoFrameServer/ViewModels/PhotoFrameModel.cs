namespace PhotoFrameServer.ViewModels;

public class PhotoFrameModel
{
    public int ConfigRefreshIntervalSeconds { get; set; }
    public int PhotoSwitchIntervalSeconds { get; set; }
    public List<PhotoModel> Photos { get; } = new();

    public PhotoFrameModel()
    {
    }
}
