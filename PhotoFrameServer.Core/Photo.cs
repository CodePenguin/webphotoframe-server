namespace PhotoFrameServer.Core;

public class Photo : IPhoto
{
    public byte[] FileContents { get; }
    public string FileExtension { get; }

    public Photo(byte[] fileContents, string fileExtension)
    {
        FileContents = fileContents;
        FileExtension = fileExtension;
    }
}
