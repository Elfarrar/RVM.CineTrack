using Microsoft.EntityFrameworkCore;
using RVM.CineTrack.Domain.Entities;
using RVM.CineTrack.Domain.Enums;
using RVM.CineTrack.Infrastructure.Data;
using RVM.CineTrack.Infrastructure.Services;

namespace RVM.CineTrack.API.Workers;

/// <summary>
/// Background worker that checks for upcoming releases and creates alerts.
/// Runs once daily. Creates alerts 7 days before release for media in users' watchlists.
/// </summary>
public class ReleaseAlertWorker(IServiceScopeFactory scopeFactory, ILogger<ReleaseAlertWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for startup
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckReleasesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking releases");
            }

            // Run daily at 08:00 UTC
            var now = DateTime.UtcNow;
            var nextRun = now.Date.AddDays(1).AddHours(8);
            if (now.Hour < 8) nextRun = now.Date.AddHours(8);

            var delay = nextRun - now;
            logger.LogInformation("Next release check at {NextRun} (in {Delay})", nextRun, delay);
            await Task.Delay(delay, stoppingToken);
        }
    }

    private async Task CheckReleasesAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CineTrackDbContext>();
        var tmdb = scope.ServiceProvider.GetRequiredService<TmdbClient>();

        var alertWindow = DateTime.UtcNow.Date.AddDays(7);
        var today = DateTime.UtcNow.Date;

        // Find TV series in any user's watchlist that are "Watching" or "WantToWatch"
        var trackedSeries = await db.WatchListItems
            .Include(w => w.Media)
            .Where(w => w.Media.Type == MediaType.TvSeries &&
                        (w.Status == WatchStatus.Watching || w.Status == WatchStatus.WantToWatch))
            .Select(w => new { w.UserId, w.MediaId, w.Media.TmdbId, w.Media.Title })
            .Distinct()
            .ToListAsync(ct);

        foreach (var item in trackedSeries)
        {
            // Check seasons for upcoming air dates
            var seasons = await db.Seasons
                .Where(s => s.MediaId == item.MediaId && s.AirDate != null &&
                            s.AirDate.Value.Date >= today && s.AirDate.Value.Date <= alertWindow)
                .ToListAsync(ct);

            foreach (var season in seasons)
            {
                var alreadyAlerted = await db.ReleaseAlerts.AnyAsync(a =>
                    a.UserId == item.UserId && a.MediaId == item.MediaId &&
                    a.ReleaseDate == season.AirDate!.Value, ct);

                if (!alreadyAlerted)
                {
                    db.ReleaseAlerts.Add(new ReleaseAlert
                    {
                        UserId = item.UserId,
                        MediaId = item.MediaId,
                        Title = $"{item.Title} - {season.Name}",
                        Description = $"{season.Name} estreia em {season.AirDate!.Value:dd/MM/yyyy}",
                        ReleaseDate = season.AirDate.Value
                    });

                    logger.LogInformation("Created release alert: {Title} S{Season} for user {UserId}",
                        item.Title, season.SeasonNumber, item.UserId);
                }
            }
        }

        // Check upcoming movies in watchlists
        var trackedMovies = await db.WatchListItems
            .Include(w => w.Media)
            .Where(w => w.Media.Type == MediaType.Movie &&
                        w.Status == WatchStatus.WantToWatch &&
                        w.Media.ReleaseDate != null &&
                        w.Media.ReleaseDate.Value.Date >= today &&
                        w.Media.ReleaseDate.Value.Date <= alertWindow)
            .Select(w => new { w.UserId, w.MediaId, w.Media.Title, w.Media.ReleaseDate })
            .ToListAsync(ct);

        foreach (var item in trackedMovies)
        {
            var alreadyAlerted = await db.ReleaseAlerts.AnyAsync(a =>
                a.UserId == item.UserId && a.MediaId == item.MediaId &&
                a.ReleaseDate == item.ReleaseDate!.Value, ct);

            if (!alreadyAlerted)
            {
                db.ReleaseAlerts.Add(new ReleaseAlert
                {
                    UserId = item.UserId,
                    MediaId = item.MediaId,
                    Title = item.Title,
                    Description = $"{item.Title} estreia em {item.ReleaseDate!.Value:dd/MM/yyyy}",
                    ReleaseDate = item.ReleaseDate.Value
                });

                logger.LogInformation("Created release alert: {Title} for user {UserId}",
                    item.Title, item.UserId);
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
