using PhotoFrameServer.Core;

namespace PhotoFrameServer.Configuration;

public class PhotoProviderContext : IPhotoProviderContext
{
    private string _data = string.Empty;

    public string Data
    {
        get => _data;
        set
        {
            _data = value;
            Modified = true;
        }
    }
    public bool Modified { get; private set; } = false;
    public Dictionary<string, object> Settings { get; private set; }

    public PhotoProviderContext (string data, Dictionary<string, object> settings)
    {
        _data = data;
        Settings = settings;
    }

    IReadOnlyDictionary<string, object> IPhotoProviderContext.Settings => Settings;
}

