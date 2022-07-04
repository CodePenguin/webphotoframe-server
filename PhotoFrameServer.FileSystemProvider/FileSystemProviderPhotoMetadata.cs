using PhotoFrameServer.Core;

namespace PhotoFrameServer;

internal class FileSystemProviderPhotoMetadata : IPhotoMetadata
{
    public string ExternalId { get; set; }
    public string Filename { get; set; }
    public string FileExtension => Path.GetExtension(Filename);

    public FileSystemProviderPhotoMetadata(string externalId, string filename)
    {
        ExternalId = externalId;
        Filename = filename;
    }
}
