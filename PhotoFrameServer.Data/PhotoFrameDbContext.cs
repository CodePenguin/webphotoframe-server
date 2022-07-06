using Microsoft.EntityFrameworkCore;

namespace PhotoFrameServer.Data;

public class PhotoFrameDbContext : DbContext
{
    public DbSet<Photo> Photos => Set<Photo>();
    public DbSet<PhotoFrame> PhotoFrames => Set<PhotoFrame>();
    public DbSet<PhotoFrameSlot> PhotoFrameSlots => Set<PhotoFrameSlot>();
    public DbSet<PhotoProviderInstance> PhotoProviderInstances => Set<PhotoProviderInstance>();

    public PhotoFrameDbContext(DbContextOptions<PhotoFrameDbContext> options) : base(options)
    {
    }

    public PhotoFrame AddOrGetPhotoFrame(string photoFrameId)
    {
        var photoFrame = PhotoFrames.Include(p => p.Slots).SingleOrDefault(p => p.Id == photoFrameId)
            ?? ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Added && x.Entity is PhotoFrame p && p.Id == photoFrameId)
                .Select(x => x.Entity as PhotoFrame)
                .SingleOrDefault();
        if (photoFrame is null)
        {
            photoFrame = new PhotoFrame
            {
                Id = photoFrameId
            };
            PhotoFrames.Add(photoFrame);
        }
        return photoFrame;
    }

    public Photo? GetPhotoByExternalId(string externalId)
    {
        return Photos.SingleOrDefault(p => p.ExternalId == externalId)
            ?? ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Added && x.Entity is Photo p && p.ExternalId == externalId)
                .Select(x => x.Entity as Photo)
                .SingleOrDefault();
    }

    public string GetPhotoProviderInstanceData(string photoFrameId, string photoProviderInstanceId)
    {
        var instance = PhotoProviderInstances.SingleOrDefault(p => p.PhotoFrameId == photoFrameId && p.Id == photoProviderInstanceId);
        return instance?.Data ?? string.Empty;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Photo>()
            .HasAlternateKey(c => c.ExternalId);
        modelBuilder.Entity<PhotoFrameSlot>()
            .HasAlternateKey(p => new { p.PhotoFrameId, p.PhotoId });
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

