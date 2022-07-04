namespace PhotoFrameServer.Core;

public interface IPhoto
{
    public byte[] FileContents { get; }
    public string FileExtension { get; }
}
