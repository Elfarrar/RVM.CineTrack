using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RVM.CineTrack.Domain.Entities;
using RVM.CineTrack.Infrastructure.Data;

namespace RVM.CineTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReviewsController(CineTrackDbContext db) : ControllerBase
{
    public record CreateReviewRequest(int UserId, int Rating, string? Comment, int? MediaId, int? SeasonId, int? EpisodeId);

    [HttpGet("media/{mediaId:int}")]
    public async Task<IActionResult> GetMediaReviews(int mediaId)
    {
        var reviews = await db.Reviews
            .Include(r => r.User)
            .Where(r => r.MediaId == mediaId && r.SeasonId == null && r.EpisodeId == null)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id, r.Rating, r.Comment, r.CreatedAt,
                user = new { r.User.Username, r.User.DisplayName, r.User.AvatarUrl }
            })
            .ToListAsync();

        return Ok(reviews);
    }

    [HttpGet("season/{seasonId:int}")]
    public async Task<IActionResult> GetSeasonReviews(int seasonId)
    {
        var reviews = await db.Reviews
            .Include(r => r.User)
            .Where(r => r.SeasonId == seasonId && r.EpisodeId == null)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id, r.Rating, r.Comment, r.CreatedAt,
                user = new { r.User.Username, r.User.DisplayName, r.User.AvatarUrl }
            })
            .ToListAsync();

        return Ok(reviews);
    }

    [HttpGet("episode/{episodeId:int}")]
    public async Task<IActionResult> GetEpisodeReviews(int episodeId)
    {
        var reviews = await db.Reviews
            .Include(r => r.User)
            .Where(r => r.EpisodeId == episodeId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id, r.Rating, r.Comment, r.CreatedAt,
                user = new { r.User.Username, r.User.DisplayName, r.User.AvatarUrl }
            })
            .ToListAsync();

        return Ok(reviews);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReviewRequest request)
    {
        if (request.Rating < 1 || request.Rating > 5)
            return BadRequest(new { error = "Rating must be between 1 and 5 oscars." });

        if (request.MediaId is null && request.SeasonId is null && request.EpisodeId is null)
            return BadRequest(new { error = "Must specify mediaId, seasonId, or episodeId." });

        var existing = await db.Reviews.FirstOrDefaultAsync(r =>
            r.UserId == request.UserId &&
            r.MediaId == request.MediaId &&
            r.SeasonId == request.SeasonId &&
            r.EpisodeId == request.EpisodeId);

        if (existing is not null)
        {
            existing.Rating = request.Rating;
            existing.Comment = request.Comment;
            existing.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Ok(existing);
        }

        var review = new Review
        {
            UserId = request.UserId,
            MediaId = request.MediaId,
            SeasonId = request.SeasonId,
            EpisodeId = request.EpisodeId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        db.Reviews.Add(review);
        await db.SaveChangesAsync();

        return Created($"/api/reviews/media/{request.MediaId}", review);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var review = await db.Reviews.FindAsync(id);
        if (review is null) return NotFound();

        db.Reviews.Remove(review);
        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("rankings/top")]
    [AllowAnonymous]
    public async Task<IActionResult> TopRated([FromQuery] int limit = 20)
    {
        var top = await db.Reviews
            .Where(r => r.MediaId != null && r.SeasonId == null && r.EpisodeId == null)
            .GroupBy(r => r.MediaId)
            .Select(g => new { MediaId = g.Key, Avg = g.Average(r => r.Rating), Count = g.Count() })
            .Where(x => x.Count >= 2)
            .OrderByDescending(x => x.Avg)
            .ThenByDescending(x => x.Count)
            .Take(limit)
            .ToListAsync();

        var mediaIds = top.Select(t => t.MediaId).ToList();
        var mediaMap = await db.Media
            .Where(m => mediaIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id);

        var result = top.Select(t => new
        {
            mediaId = t.MediaId,
            averageRating = Math.Round(t.Avg, 2),
            reviewCount = t.Count,
            media = mediaMap.TryGetValue(t.MediaId!.Value, out var m) ? new
            {
                m.Title, m.PosterPath, m.Type, m.Genres
            } : null
        });

        return Ok(result);
    }

    [HttpGet("rankings/worst")]
    [AllowAnonymous]
    public async Task<IActionResult> WorstRated([FromQuery] int limit = 20)
    {
        var worst = await db.Reviews
            .Where(r => r.MediaId != null && r.SeasonId == null && r.EpisodeId == null)
            .GroupBy(r => r.MediaId)
            .Select(g => new { MediaId = g.Key, Avg = g.Average(r => r.Rating), Count = g.Count() })
            .Where(x => x.Count >= 2)
            .OrderBy(x => x.Avg)
            .ThenByDescending(x => x.Count)
            .Take(limit)
            .ToListAsync();

        var mediaIds = worst.Select(t => t.MediaId).ToList();
        var mediaMap = await db.Media
            .Where(m => mediaIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id);

        var result = worst.Select(t => new
        {
            mediaId = t.MediaId,
            averageRating = Math.Round(t.Avg, 2),
            reviewCount = t.Count,
            media = mediaMap.TryGetValue(t.MediaId!.Value, out var m) ? new
            {
                m.Title, m.PosterPath, m.Type, m.Genres
            } : null
        });

        return Ok(result);
    }
}
