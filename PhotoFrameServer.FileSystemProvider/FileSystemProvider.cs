using PhotoFrameServer.Core;
using Microsoft.Extensions.Logging;

namespace PhotoFrameServer;

public class FileSystemProvider : PhotoProviderBase, IPhotoProvider
{
    private readonly Random _random = new();
    private FileSystemProviderData _data = null!;
    private FileSystemProviderSettings _settings = null!;

    public FileSystemProvider(ILogger<FileSystemProvider> logger) : base(logger)
    {
    }

    public override void Initialize(IPhotoProviderContext context)
    {
        base.Initialize(context);
        _data = Context.GetData<FileSystemProviderData>();
        _settings = Context.GetSettings<FileSystemProviderSettings>();
    }

    public override void Deinitialize(IPhotoProviderContext context)
    {
        Context.SetData(_data);
        base.Deinitialize(context);
    }

    public override IEnumerable<byte> GetPhotoContents(IPhotoMetadata photoMetadata)
    {
        if (photoMetadata is not FileSystemProviderPhotoMetadata providerPhotoMetadata)
        {
            return Array.Empty<byte>();
        }
        var filename = Path.Combine(_settings.Path, providerPhotoMetadata.Filename);
        return File.Exists(filename)
            ? File.ReadAllBytes(Path.Combine(_settings.Path, providerPhotoMetadata.Filename))
            : Array.Empty<byte>();
    }

    public override IEnumerable<IPhotoMetadata> GetPhotos(int photoLimit)
    {
        var output = new List<IPhotoMetadata>();

        if (string.IsNullOrWhiteSpace(_settings.Path))
        {
            throw new ArgumentException("Path is required.");
        }

        var enumerationOptions = new EnumerationOptions
        {
            IgnoreInaccessible = true,
            RecurseSubdirectories = _settings.IncludeSubFolders
        };

        var photoFilenames = new List<string>();
        foreach (var filename in Directory.EnumerateFiles(_settings.Path, "*.*", enumerationOptions))
        {
            if (filename is null)
            {
                continue;
            }
            if (!PhotoProviderConstants.SupportedPhotoFileExtensions.Contains(Path.GetExtension(filename).ToLowerInvariant()))
            {
                continue;
            }
            var relativeFilename = Path.GetRelativePath(_settings.Path, filename);
            photoFilenames.Add(relativeFilename);
        }

        var index = (photoFilenames.IndexOf(_data.LastFilename) + 1) % photoFilenames.Count;
        while (output.Count < photoLimit && photoFilenames.Count > 0)
        {
            if (_settings.Random)
            {
                index = _random.Next(0, photoFilenames.Count);
            }
            var filename = photoFilenames[index];
            var externalId = GenerateExternalId(filename);
            var photo = new FileSystemProviderPhotoMetadata(externalId, filename);
            output.Add(photo);
            photoFilenames.RemoveAt(index);
            _data.LastFilename = filename;
        }

        return output;
    }
}

