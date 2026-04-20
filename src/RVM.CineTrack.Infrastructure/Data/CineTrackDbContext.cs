namespace RVM.CineTrack.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using RVM.CineTrack.Domain.Entities;

public class CineTrackDbContext : DbContext
{
    public CineTrackDbContext(DbContextOptions<CineTrackDbContext> options) : base(options) { }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Media> Media => Set<Media>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<Episode> Episodes => Set<Episode>();
    public DbSet<WatchListItem> WatchListItems => Set<WatchListItem>();
    public DbSet<EpisodeWatch> EpisodeWatches => Set<EpisodeWatch>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ReleaseAlert> ReleaseAlerts => Set<ReleaseAlert>();
    public DbSet<MediaCast> MediaCast => Set<MediaCast>();
    public DbSet<WatchProvider> WatchProviders => Set<WatchProvider>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(e =>
        {
            e.HasIndex(u => u.ExternalId).IsUnique();
            e.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<Media>(e =>
        {
            e.HasIndex(m => new { m.TmdbId, m.Type }).IsUnique();
        });

        modelBuilder.Entity<Season>(e =>
        {
            e.HasIndex(s => new { s.MediaId, s.SeasonNumber }).IsUnique();
        });

        modelBuilder.Entity<Episode>(e =>
        {
            e.HasIndex(ep => new { ep.SeasonId, ep.EpisodeNumber }).IsUnique();
        });

        modelBuilder.Entity<WatchListItem>(e =>
        {
            e.HasIndex(w => new { w.UserId, w.MediaId }).IsUnique();
        });

        modelBuilder.Entity<EpisodeWatch>(e =>
        {
            e.HasIndex(ew => new { ew.UserId, ew.EpisodeId }).IsUnique();
        });

        modelBuilder.Entity<Review>(e =>
        {
            e.HasIndex(r => new { r.UserId, r.MediaId, r.SeasonId, r.EpisodeId }).IsUnique();
            e.Property(r => r.Rating).HasAnnotation("Range", new[] { 1, 5 });
        });

        modelBuilder.Entity<MediaCast>(e =>
        {
            e.HasIndex(mc => new { mc.MediaId, mc.TmdbPersonId, mc.Department }).IsUnique();
        });

        modelBuilder.Entity<WatchProvider>(e =>
        {
            e.HasIndex(wp => new { wp.MediaId, wp.TmdbProviderId, wp.Type }).IsUnique();
        });
    }
}
