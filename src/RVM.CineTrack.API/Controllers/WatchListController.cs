using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RVM.CineTrack.API.Services;
using RVM.CineTrack.Domain.Entities;
using RVM.CineTrack.Domain.Enums;
using RVM.CineTrack.Infrastructure.Data;

namespace RVM.CineTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WatchListController(CineTrackDbContext db, MediaSyncService sync) : ControllerBase
{
    public record AddToWatchListRequest(int TmdbId, string MediaType, WatchStatus Status = WatchStatus.WantToWatch);
    public record UpdateStatusRequest(WatchStatus Status, int? Rating);

    [HttpGet("{userId:int}")]
    public async Task<IActionResult> GetUserWatchList(int userId, [FromQuery] WatchStatus? status)
    {
        var query = db.WatchListItems
            .Include(w => w.Media)
            .Where(w => w.UserId == userId);

        if (status.HasValue)
            query = query.Where(w => w.Status == status.Value);

        var items = await query
            .OrderByDescending(w => w.UpdatedAt)
            .Select(w => new
            {
                w.Id,
                w.MediaId,
                w.Status,
                w.Rating,
                w.StartedAt,
                w.FinishedAt,
                w.CreatedAt,
                media = new
                {
                    w.Media.Title,
                    w.Media.PosterPath,
                    w.Media.Type,
                    w.Media.TmdbRating,
                    w.Media.ReleaseDate,
                    w.Media.Genres
                }
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddToWatchListRequest request, [FromQuery] int userId)
    {
        // Sync media from TMDB if not local
        Media? media;
        var mediaType = request.MediaType == "movie" ? MediaType.Movie : MediaType.TvSeries;
        media = await db.Media.FirstOrDefaultAsync(m => m.TmdbId == request.TmdbId && m.Type == mediaType);

        if (media is null)
        {
            media = mediaType == MediaType.Movie
                ? await sync.SyncMovieAsync(request.TmdbId)
                : await sync.SyncTvSeriesAsync(request.TmdbId, syncEpisodes: true);
        }

        if (media is null)
            return StatusCode(502, new { error = "Could not fetch media from TMDB." });

        var exists = await db.WatchListItems.AnyAsync(w => w.UserId == userId && w.MediaId == media.Id);
        if (exists)
            return Conflict(new { error = "Media already in watchlist." });

        var item = new WatchListItem
        {
            UserId = userId,
            MediaId = media.Id,
            Status = request.Status,
            StartedAt = request.Status == WatchStatus.Watching ? DateTime.UtcNow : null
        };

        db.WatchListItems.Add(item);
        await db.SaveChangesAsync();

        return Created($"/api/watchlist/{userId}", item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        var item = await db.WatchListItems.FindAsync(id);
        if (item is null) return NotFound();

        item.Status = request.Status;
        if (request.Rating.HasValue)
            item.Rating = Math.Clamp(request.Rating.Value, 1, 5);

        if (request.Status == WatchStatus.Watching && item.StartedAt is null)
            item.StartedAt = DateTime.UtcNow;
        if (request.Status == WatchStatus.Watched && item.FinishedAt is null)
            item.FinishedAt = DateTime.UtcNow;

        item.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return Ok(item);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remove(int id)
    {
        var item = await db.WatchListItems.FindAsync(id);
        if (item is null) return NotFound();

        db.WatchListItems.Remove(item);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
