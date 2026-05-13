using FitnessApp.Api.Models;
using Microsoft.EntityFrameworkCore;
namespace FitnessApp.Api.Data;
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<WorkoutVideo> WorkoutVideos => Set<WorkoutVideo>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkoutVideo>(entity =>
            {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.YoutubeVideoId).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.YoutubeVideoId).IsUnique();
            entity.Property(x => x.YoutubeUrl).HasMaxLength(300).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(300).IsRequired();
            entity.Property(x => x.ChannelTitle).HasMaxLength(200);
            entity.Property(x => x.ThumbnailUrl).HasMaxLength(500);
            });
    }
}
 