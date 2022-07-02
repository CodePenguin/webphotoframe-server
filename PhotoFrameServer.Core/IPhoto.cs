namespace PhotoFrameServer.Core;

public interface IPhoto
{
    public string ExternalId { get; }
    public byte[] FileContents { get; }
    public string FileExtension { get; }
}
