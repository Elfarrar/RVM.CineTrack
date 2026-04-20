using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RVM.CineTrack.Domain.Enums;
using RVM.CineTrack.Infrastructure.Data;

namespace RVM.CineTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatsController(CineTrackDbContext db) : ControllerBase
{
    [HttpGet("{userId:int}")]
    public async Task<IActionResult> GetUserStats(int userId)
    {
        var watchedMovies = await db.WatchListItems
            .Where(w => w.UserId == userId && w.Status == WatchStatus.Watched && w.Media.Type == MediaType.Movie)
            .CountAsync();

        var watchedSeries = await db.WatchListItems
            .Where(w => w.UserId == userId && w.Status == WatchStatus.Watched && w.Media.Type == MediaType.TvSeries)
            .CountAsync();

        var episodesWatched = await db.EpisodeWatches
            .Where(ew => ew.UserId == userId)
            .CountAsync();

        // Hours watched (movies runtime + episodes runtime)
        var movieMinutes = await db.WatchListItems
            .Where(w => w.UserId == userId && w.Status == WatchStatus.Watched && w.Media.Runtime != null)
            .SumAsync(w => w.Media.Runtime!.Value);

        var episodeMinutes = await db.EpisodeWatches
            .Where(ew => ew.UserId == userId && ew.Episode.Runtime != null)
            .SumAsync(ew => ew.Episode.Runtime!.Value);

        var totalHours = Math.Round((movieMinutes + episodeMinutes) / 60.0, 1);

        // Average rating
        var avgRating = await db.Reviews
            .Where(r => r.UserId == userId)
            .AverageAsync(r => (double?)r.Rating) ?? 0;

        // Top genres
        var genreData = await db.WatchListItems
            .Where(w => w.UserId == userId && w.Status == WatchStatus.Watched && w.Media.Genres != null)
            .Select(w => w.Media.Genres!)
            .ToListAsync();

        var topGenres = genreData
            .SelectMany(g => g.Split(", ", StringSplitOptions.RemoveEmptyEntries))
            .GroupBy(g => g)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => new { genre = g.Key, count = g.Count() });

        // Most watched artists
        var artistData = await db.WatchListItems
            .Where(w => w.UserId == userId && w.Status == WatchStatus.Watched)
            .Select(w => w.MediaId)
            .ToListAsync();

        var topArtists = await db.MediaCast
            .Where(mc => artistData.Contains(mc.MediaId) && mc.Department == "Acting")
            .GroupBy(mc => new { mc.TmdbPersonId, mc.Name, mc.ProfilePath })
            .Select(g => new { g.Key.Name, g.Key.ProfilePath, count = g.Count() })
            .OrderByDescending(x => x.count)
            .Take(10)
            .ToListAsync();

        // Streak (consecutive days with episode watches)
        var watchDates = await db.EpisodeWatches
            .Where(ew => ew.UserId == userId)
            .Select(ew => ew.WatchedAt.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToListAsync();

        var streak = 0;
        var today = DateTime.UtcNow.Date;
        foreach (var date in watchDates)
        {
            if (date == today.AddDays(-streak))
                streak++;
            else
                break;
        }

        return Ok(new
        {
            watchedMovies,
            watchedSeries,
            episodesWatched,
            totalHours,
            averageRating = Math.Round(avgRating, 2),
            currentStreak = streak,
            topGenres,
            topArtists
        });
    }
}
