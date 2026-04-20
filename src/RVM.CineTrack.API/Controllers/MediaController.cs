using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RVM.CineTrack.API.Services;
using RVM.CineTrack.Domain.Enums;
using RVM.CineTrack.Infrastructure.Data;

namespace RVM.CineTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MediaController(CineTrackDbContext db, MediaSyncService sync) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var media = await db.Media
            .Include(m => m.Seasons.OrderBy(s => s.SeasonNumber))
            .Include(m => m.Cast.OrderBy(c => c.Order))
            .Include(m => m.WatchProviders)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (media is null)
            return NotFound();

        return Ok(media);
    }

    [HttpPost("sync/movie/{tmdbId:int}")]
    public async Task<IActionResult> SyncMovie(int tmdbId)
    {
        var media = await sync.SyncMovieAsync(tmdbId);
        if (media is null)
            return StatusCode(502, new { error = "Could not fetch from TMDB." });

        return Ok(media);
    }

    [HttpPost("sync/tv/{tmdbId:int}")]
    public async Task<IActionResult> SyncTv(int tmdbId, [FromQuery] bool episodes = false)
    {
        var media = await sync.SyncTvSeriesAsync(tmdbId, episodes);
        if (media is null)
            return StatusCode(502, new { error = "Could not fetch from TMDB." });

        return Ok(media);
    }

    [HttpGet("{id:int}/seasons/{seasonNumber:int}/episodes")]
    public async Task<IActionResult> GetEpisodes(int id, int seasonNumber)
    {
        var season = await db.Seasons
            .Include(s => s.Episodes.OrderBy(e => e.EpisodeNumber))
            .FirstOrDefaultAsync(s => s.MediaId == id && s.SeasonNumber == seasonNumber);

        if (season is null)
            return NotFound();

        return Ok(season.Episodes);
    }

    [HttpGet("{id:int}/providers")]
    public async Task<IActionResult> GetProviders(int id)
    {
        var providers = await db.WatchProviders
            .Where(wp => wp.MediaId == id)
            .OrderBy(wp => wp.Type)
            .ThenBy(wp => wp.Name)
            .ToListAsync();

        return Ok(providers);
    }
}
