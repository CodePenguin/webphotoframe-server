using Microsoft.EntityFrameworkCore;

namespace PhotoFrameServer.Data;

public class PhotoFrameContext : DbContext
{
    public DbSet<Photo> Photos => Set<Photo>();
    public DbSet<PhotoFrame> PhotoFrames => Set<PhotoFrame>();

    public PhotoFrameContext(DbContextOptions<PhotoFrameContext> options) : base(options)
    {
    }
}

