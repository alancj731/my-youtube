using Microsoft.EntityFrameworkCore;
using MyYt.Infrastructure.Persistence.Entities;

namespace MyYt.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<VideoEntity> Videos => Set<VideoEntity>();
    public DbSet<TagEntity> Tags => Set<TagEntity>();
    public DbSet<VideoTagEntity> VideoTags => Set<VideoTagEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
