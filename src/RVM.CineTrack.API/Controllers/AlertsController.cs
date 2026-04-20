using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RVM.CineTrack.Infrastructure.Data;

namespace RVM.CineTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AlertsController(CineTrackDbContext db) : ControllerBase
{
    [HttpGet("{userId:int}")]
    public async Task<IActionResult> GetAlerts(int userId, [FromQuery] bool unreadOnly = false)
    {
        var query = db.ReleaseAlerts
            .Include(a => a.Media)
            .Where(a => a.UserId == userId);

        if (unreadOnly)
            query = query.Where(a => !a.IsRead);

        var alerts = await query
            .OrderByDescending(a => a.ReleaseDate)
            .Select(a => new
            {
                a.Id, a.Title, a.Description, a.ReleaseDate, a.IsRead, a.CreatedAt,
                media = new { a.Media.Title, a.Media.PosterPath, a.Media.Type }
            })
            .ToListAsync();

        return Ok(alerts);
    }

    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var alert = await db.ReleaseAlerts.FindAsync(id);
        if (alert is null) return NotFound();

        alert.IsRead = true;
        await db.SaveChangesAsync();

        return Ok();
    }

    [HttpPut("{userId:int}/read-all")]
    public async Task<IActionResult> MarkAllRead(int userId)
    {
        await db.ReleaseAlerts
            .Where(a => a.UserId == userId && !a.IsRead)
            .ExecuteUpdateAsync(a => a.SetProperty(x => x.IsRead, true));

        return Ok();
    }
}
