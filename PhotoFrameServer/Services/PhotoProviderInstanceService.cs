using System.Text.Json;
using PhotoFrameServer.Data;

namespace PhotoFrameServer.Services;

public class PhotoProviderInstanceService
{
    private readonly PhotoFrameDbContext _db;

    public PhotoProviderInstanceService(PhotoFrameDbContext db)
    {
        _db = db;
    }

    public Dictionary<string, object> GetData(string photoFrameId, string photoProviderInstanceId)
    {
        var instance = _db.PhotoProviderInstances.SingleOrDefault(p => p.PhotoFrameId == photoFrameId && p.Id == photoProviderInstanceId);
        if (instance is null || string.IsNullOrWhiteSpace(instance.Data))
        {
            return new Dictionary<string, object>();
        }
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(instance.Data);
        return dictionary ?? new Dictionary<string, object>();
    }

    public void SaveData(string photoFrameId, string photoProviderInstanceId, Dictionary<string, object> data)
    {
        var instance = _db.PhotoProviderInstances.SingleOrDefault(p => p.PhotoFrameId == photoFrameId && p.Id == photoProviderInstanceId);
        if (instance is null)
        {
            instance = new PhotoProviderInstance
            {
                PhotoFrameId = photoFrameId,
                Id = photoProviderInstanceId
            };
            _db.Add(instance);
        }
        var serializedData = JsonSerializer.Serialize(data);
        instance.Data = serializedData;
    }

    public void SaveAllChanges()
    {
        _db.SaveChanges();
    }
}

