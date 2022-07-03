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
}

