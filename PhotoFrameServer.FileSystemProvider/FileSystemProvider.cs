using PhotoFrameServer.Core;
using Microsoft.Extensions.Logging;

namespace PhotoFrameServer;

public class FileSystemProvider : PhotoProviderBase, IPhotoProvider
{
    private readonly Random _random = new();

    public FileSystemProvider(ILogger<FileSystemProvider> logger) : base(logger)
    {
    }

    public override IEnumerable<IPhoto> GetPhotos(int photoLimit)
    {
        var output = new List<Photo>();
        var settings = Context.GetSettings<FileSystemProviderSettings>();
        var data = Context.GetData<FileSystemProviderData>();

        if (string.IsNullOrWhiteSpace(settings.Path))
        {
            throw new ArgumentException("Path is required.");
        }
        var enumerationOptions = new EnumerationOptions
        {
            IgnoreInaccessible = true,
            RecurseSubdirectories = settings.IncludeSubFolders
        };
        var photoFilenames = new List<string>();
        foreach (var filename in Directory.EnumerateFiles(settings.Path, "*.*", enumerationOptions))
        {
            if (filename is null)
            {
                continue;
            }
            if (!PhotoProviderConstants.SupportedPhotoFileExtensions.Contains(Path.GetExtension(filename).ToLowerInvariant()))
            {
                continue;
            }
            var relativeFilename = Path.GetRelativePath(settings.Path, filename);
            photoFilenames.Add(relativeFilename);
        }

        while (output.Count < photoLimit && photoFilenames.Count > 0)
        {
            var index = _random.Next(0, photoFilenames.Count);
            var fileContents = File.ReadAllBytes(Path.Combine(settings.Path, photoFilenames[index]));
            var fileExtension = Path.GetExtension(photoFilenames[index]);
            var photo = new Photo(fileContents, fileExtension);
            output.Add(photo);
            photoFilenames.RemoveAt(index);
        }

        Context.SetData(data);

        return output;
    }
}

