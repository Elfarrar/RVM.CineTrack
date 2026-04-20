using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RVM.CineTrack.Domain.Entities;
using RVM.CineTrack.Infrastructure.Data;

namespace RVM.CineTrack.API.Controllers;

[ApiController]
[Route("api/episodes")]
[Authorize]
public class EpisodeWatchController(CineTrackDbContext db) : ControllerBase
{
    [HttpGet("{userId:int}/watched")]
    public async Task<IActionResult> GetWatched(int userId, [FromQuery] int? seasonId)
    {
        var query = db.EpisodeWatches
            .Include(ew => ew.Episode)
            .Where(ew => ew.UserId == userId);

        if (seasonId.HasValue)
            query = query.Where(ew => ew.Episode.SeasonId == seasonId.Value);

        var watched = await query
            .OrderBy(ew => ew.Episode.EpisodeNumber)
            .Select(ew => new
            {
                ew.EpisodeId,
                ew.Episode.EpisodeNumber,
                ew.Episode.Name,
                ew.WatchedAt
            })
            .ToListAsync();

        return Ok(watched);
    }

    [HttpPost("{userId:int}/watch/{episodeId:int}")]
    public async Task<IActionResult> MarkWatched(int userId, int episodeId)
    {
        var episode = await db.Episodes.FindAsync(episodeId);
        if (episode is null) return NotFound(new { error = "Episode not found." });

        var exists = await db.EpisodeWatches.AnyAsync(ew => ew.UserId == userId && ew.EpisodeId == episodeId);
        if (exists) return Ok(new { message = "Already watched." });

        db.EpisodeWatches.Add(new EpisodeWatch { UserId = userId, EpisodeId = episodeId });
        await db.SaveChangesAsync();

        return Created();
    }

    [HttpDelete("{userId:int}/watch/{episodeId:int}")]
    public async Task<IActionResult> UnmarkWatched(int userId, int episodeId)
    {
        var watch = await db.EpisodeWatches
            .FirstOrDefaultAsync(ew => ew.UserId == userId && ew.EpisodeId == episodeId);

        if (watch is null) return NotFound();

        db.EpisodeWatches.Remove(watch);
        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{userId:int}/watch-season/{seasonId:int}")]
    public async Task<IActionResult> MarkSeasonWatched(int userId, int seasonId)
    {
        var episodes = await db.Episodes
            .Where(e => e.SeasonId == seasonId)
            .Select(e => e.Id)
            .ToListAsync();

        var alreadyWatched = await db.EpisodeWatches
            .Where(ew => ew.UserId == userId && episodes.Contains(ew.EpisodeId))
            .Select(ew => ew.EpisodeId)
            .ToListAsync();

        var toAdd = episodes.Except(alreadyWatched)
            .Select(epId => new EpisodeWatch { UserId = userId, EpisodeId = epId });

        db.EpisodeWatches.AddRange(toAdd);
        await db.SaveChangesAsync();

        return Ok(new { marked = episodes.Count - alreadyWatched.Count });
    }
}
