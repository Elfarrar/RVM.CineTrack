using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RVM.CineTrack.Domain.Entities;
using RVM.CineTrack.Infrastructure.Data;

namespace RVM.CineTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(CineTrackDbContext db) : ControllerBase
{
    public record CreateUserRequest(string ExternalId, string Username, string? DisplayName, string? AvatarUrl);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();
        return Ok(user);
    }

    [HttpGet("by-external/{externalId}")]
    public async Task<IActionResult> GetByExternalId(string externalId)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.ExternalId == externalId);
        if (user is null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var exists = await db.Users.AnyAsync(u => u.ExternalId == request.ExternalId);
        if (exists)
            return Conflict(new { error = "User already exists." });

        var user = new AppUser
        {
            ExternalId = request.ExternalId,
            Username = request.Username,
            DisplayName = request.DisplayName,
            AvatarUrl = request.AvatarUrl
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Created($"/api/users/{user.Id}", user);
    }

    [HttpGet("community/recent")]
    [AllowAnonymous]
    public async Task<IActionResult> RecentActivity([FromQuery] int limit = 20)
    {
        var recent = await db.Reviews
            .Include(r => r.User)
            .Include(r => r.Media)
            .Where(r => r.Media != null)
            .OrderByDescending(r => r.CreatedAt)
            .Take(limit)
            .Select(r => new
            {
                r.Rating,
                r.Comment,
                r.CreatedAt,
                user = new { r.User.Username, r.User.DisplayName, r.User.AvatarUrl },
                media = new { r.Media!.Title, r.Media.PosterPath, r.Media.Type }
            })
            .ToListAsync();

        return Ok(recent);
    }
}
