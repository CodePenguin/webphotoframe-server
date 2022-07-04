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

    private PhotoFrameConfiguration? GetDefaultPhotoFrameConfiguration()
    {
        return _settings.PhotoFrames.OrderBy(p => p.Id).FirstOrDefault();
    }

    public async Task<PhotoFrameModel?> GetDefaultPhotoFrameAsync()
    {
        var photoFrameConfiguration = GetDefaultPhotoFrameConfiguration();
        return photoFrameConfiguration is not null
            ? await GetPhotoFrameAsync(photoFrameConfiguration.Id)
            : null;
    }

    public async Task<Photo?> GetDefaultPhotoFramePhotoAsync(Guid photoId)
    {
        var photoFrameConfiguration = GetDefaultPhotoFrameConfiguration();
        return photoFrameConfiguration is not null
            ? await GetPhotoAsync(photoFrameConfiguration.Id, photoId)
            : null;
    }

    public async Task<PhotoFrameModel?> GetPhotoFrameAsync(string photoFrameId)
    {
        var photoFrameConfiguration = _settings.PhotoFrames.SingleOrDefault(f => f.Id == photoFrameId);
        var photoFrame = await _db.PhotoFrames.Include(f => f.Slots).ThenInclude(s => s.Photo).SingleOrDefaultAsync(f => f.Id == photoFrameId);
        if (photoFrameConfiguration is null || photoFrame is null)
        {
            return null;
        }

        var model = new PhotoFrameModel
        {
            ConfigRefreshIntervalSeconds = photoFrameConfiguration.ConfigRefreshIntervalSeconds ?? _settings.DefaultConfigRefreshIntervalSeconds,
            PhotoSwitchIntervalSeconds = photoFrameConfiguration.PhotoSwitchIntervalSeconds ?? _settings.DefaultPhotoSwitchIntervalSeconds
        };

        foreach (var slot in photoFrame.Slots)
        {
            model.Photos.Add(new PhotoModel
            {
                Caption = slot.Photo.Caption,
                Url = $"photos/{slot.Photo.Id}"
            });
        }
        return model;
    }

    public async Task<Photo?> GetPhotoAsync(string photoFrameId, Guid photoId)
    {
        var slot = await _db.PhotoFrameSlots.Include(s => s.Photo).SingleOrDefaultAsync(s => s.PhotoFrameId == photoFrameId && s.PhotoId == photoId);
        if (slot is null || slot.Photo is null)
        {
            return null;
        }
        if (slot.ViewedDateTime is null)
        {
            slot.ViewedDateTime = DateTime.Now;
            _db.SaveChanges();
        }
        return slot.Photo;
    }
}
