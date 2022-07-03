using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PhotoFrameServer.Configuration;
using PhotoFrameServer.Data;
using PhotoFrameServer.ViewModels;

namespace PhotoFrameServer.Services;

public class PhotoFrameRequestHandler
{
    private readonly PhotoFrameDbContext _db;
    private readonly PhotoFramesSettings _settings;

    public PhotoFrameRequestHandler(PhotoFrameDbContext db, IOptionsSnapshot<PhotoFramesSettings> settingsSnapshot)
    {
        _db = db;
        _settings = settingsSnapshot.Value; 
    }

    public async Task<PhotoFrameModel?> GetDefaultPhotoFrameAsync()
    {
        var photoFrameSettings = _settings.PhotoFrames.FirstOrDefault();
        return photoFrameSettings is not null
            ? await GetPhotoFrameAsync(photoFrameSettings.Id)
            : null;
    }

    public async Task<PhotoFrameModel?> GetPhotoFrameAsync(string id)
    {
        var photoFrameSettings = _settings.PhotoFrames.SingleOrDefault(f => f.Id == id);
        var photoFrame = await _db.PhotoFrames.Include(f => f.Photos).SingleOrDefaultAsync(f => f.Id == id);
        if (photoFrameSettings is null || photoFrame is null)
        {
            return null;
        }

        var model = new PhotoFrameModel
        {
            ConfigRefreshIntervalSeconds = photoFrameSettings.ConfigRefreshIntervalSeconds,
            PhotoSwitchIntervalSeconds = photoFrameSettings.PhotoSwitchIntervalSeconds
        };

        foreach (var photo in photoFrame.Photos)
        {
            model.Photos.Add(new PhotoModel
            {
                Caption = photo.Caption,
                Url = $"photos/{photo.Id}"
            });
        }
        return model;
    }

    public async Task<Photo?> GetPhotoAsync(Guid id)
    {
        var photo = await _db.Photos.FindAsync(id);
        if (photo is null)
        {
            return null;
        }
        if (photo.ViewedDateTime is null)
        {
            photo.ViewedDateTime = DateTime.Now;
            _db.SaveChanges();
        }
        return photo;
    }
}
