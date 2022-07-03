using Microsoft.EntityFrameworkCore;

namespace PhotoFrameServer.Data;

public class PhotoFrameDbContext : DbContext
{
    public DbSet<Photo> Photos => Set<Photo>();
    public DbSet<PhotoFrame> PhotoFrames => Set<PhotoFrame>();
    public DbSet<PhotoProviderInstance> PhotoProviderInstances => Set<PhotoProviderInstance>();

    public PhotoFrameDbContext(DbContextOptions<PhotoFrameDbContext> options) : base(options)
    {
    }

    public string GetPhotoProviderInstanceData(string photoFrameId, string photoProviderInstanceId)
    {
        var instance = PhotoProviderInstances.SingleOrDefault(p => p.PhotoFrameId == photoFrameId && p.Id == photoProviderInstanceId);
        return instance?.Data ?? string.Empty;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<PhotoProviderInstance>()
            .HasKey(p => new { p.Id, p.PhotoFrameId });
    }

    public void SetPhotoProviderInstanceData(string photoFrameId, string photoProviderInstanceId, string data)
    {
        var instance = PhotoProviderInstances.SingleOrDefault(p => p.PhotoFrameId == photoFrameId && p.Id == photoProviderInstanceId);
        if (instance is null)
        {
            instance = new PhotoProviderInstance
            {
                PhotoFrameId = photoFrameId,
                Id = photoProviderInstanceId
            };
            Add(instance);
        }
        instance.Data = data;
    }
}

