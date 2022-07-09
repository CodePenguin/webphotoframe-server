namespace PhotoFrameServer;

internal class FileSystemProviderSettings
{
    public bool IncludeSubFolders { get; set; } = true;
    public int MaxSubFolderDepth { get; set; } = int.MaxValue; 
    public string Path { get; set; } = string.Empty;
    public bool Random { get; set; } = false;
}

