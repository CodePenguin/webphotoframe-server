using PhotoFrameServer.Core;

namespace PhotoFrameServer.Configuration;

public class PhotoProviderContext : IPhotoProviderContext
{
    public ObjectDictionary Data { get; set; } = new();
    public ObjectDictionary Settings { get; set; } = new();

    IObjectDictionary IPhotoProviderContext.Data => Data;
    IReadOnlyObjectDictionary IPhotoProviderContext.Settings => Settings;
}

