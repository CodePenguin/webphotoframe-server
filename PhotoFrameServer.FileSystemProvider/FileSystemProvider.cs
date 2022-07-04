using PhotoFrameServer.Core;
using Microsoft.Extensions.Logging;

namespace PhotoFrameServer;

public class FileSystemProvider : PhotoProviderBase, IPhotoProvider
{
    private readonly Random _random = new();
    private FileSystemProviderData? _data;

    public FileSystemProvider(ILogger<FileSystemProvider> logger) : base(logger)
    {
    }

    public override void Initialize(IPhotoProviderContext context)
    {
        base.Initialize(context);
        _data = Context.GetData<FileSystemProviderData>();
    }

    public override void Deinitialize(IPhotoProviderContext context)
    {
        Context.SetData(_data!);
        base.Deinitialize(context);
    }


    public override IEnumerable<byte> GetPhotoContents(IPhotoMetadata photoMetadata)
    {
        if (photoMetadata is not FileSystemProviderPhotoMetadata providerPhotoMetadata || !File.Exists(providerPhotoMetadata.Filename))
        {
            return Array.Empty<byte>();
        }
        return File.ReadAllBytes(providerPhotoMetadata.Filename);
    }

    public override IEnumerable<IPhotoMetadata> GetPhotos(int photoLimit)
    {
        var output = new List<IPhotoMetadata>();
        var settings = Context.GetSettings<FileSystemProviderSettings>();

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
            var filename = Path.Combine(settings.Path, photoFilenames[index]);
            var externalId = GenerateExternalId(filename);
            var photo = new FileSystemProviderPhotoMetadata(externalId, filename);
            output.Add(photo);
            photoFilenames.RemoveAt(index);
        }

        return output;
    }
}

