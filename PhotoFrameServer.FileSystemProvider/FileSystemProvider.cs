using PhotoFrameServer.Core;
using Microsoft.Extensions.Logging;

namespace PhotoFrameServer;

public class FileSystemProviderSettings
{
    public string Path { get; set; } = string.Empty;
}

public class FileSystemProviderData
{
    public int TestInt { get; set; } = 0;
    public string TestString { get; set; } = string.Empty;
}

public class FileSystemProvider : PhotoProviderBase, IPhotoProvider
{
    public FileSystemProvider(ILogger<FileSystemProvider> logger) : base(logger)
    {
    }

    public override IEnumerable<IPhoto> GetPhotos(int photoLimit)
    {
        var settings = Context.GetSettings<FileSystemProviderSettings>();
        var data = Context.GetData<FileSystemProviderData>();

        var path = Context.Settings.TryGetValue("Path", out var temp) ? temp : string.Empty;
        Logger.LogDebug("Initialized with path: {Path}", path);
        Logger.LogDebug("Initialized with path2: {Path}", settings.Path);
        Logger.LogDebug("Initialized with Data.TestInt: {Value}", data.TestInt);
        Logger.LogDebug("Initialized with Data.TestString: {Value}", data.TestString);

        data.TestInt = DateTime.Now.Millisecond;
        data.TestString = DateTime.Now.ToString();

        Context.SetData(data);

        return new List<IPhoto>();
    }

    public override void Initialize(IPhotoProviderContext context)
    {
        base.Initialize(context);
    }
}

