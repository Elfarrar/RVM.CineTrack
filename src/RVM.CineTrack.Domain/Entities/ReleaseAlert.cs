namespace RVM.CineTrack.Domain.Entities;

using RVM.CineTrack.Domain.Enums;

/// <summary>
/// Alert for upcoming release (new movie, new season, etc.).
/// Created by background worker, shown in dashboard.
/// </summary>
public class ReleaseAlert
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MediaId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; } // e.g. "Season 3 premieres in 7 days"
    public DateTime ReleaseDate { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public AppUser User { get; set; } = null!;
    public Media Media { get; set; } = null!;
}
