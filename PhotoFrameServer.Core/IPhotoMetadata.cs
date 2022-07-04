namespace PhotoFrameServer.Core;

public interface IPhotoMetadata
{
    public string ExternalId { get; }
    public string FileExtension { get; }
}
