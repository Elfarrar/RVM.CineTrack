namespace RVM.CineTrack.Domain.Entities;

using RVM.CineTrack.Domain.Enums;

/// <summary>
/// A media item in a user's watchlist with status tracking.
/// </summary>
public class WatchListItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MediaId { get; set; }
    public WatchStatus Status { get; set; } = WatchStatus.WantToWatch;
    public int? Rating { get; set; } // 1-5 oscars (overall rating)
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public AppUser User { get; set; } = null!;
    public Media Media { get; set; } = null!;
}
