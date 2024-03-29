﻿namespace PhotoFrameServer.Data;

public class Photo
{
    public Guid PhotoId { get; set; } = Guid.Empty;
    public string Caption { get; set; } = string.Empty;
    public byte[] FileContents { get; set; } = Array.Empty<byte>();
    public string FileExtension { get; set; } = string.Empty;

    public string PhotoFrameId { get; set; } = null!;
    public PhotoFrame PhotoFrame { get; set; } = null!;

    public Photo()
    {
    }
}
