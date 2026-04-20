namespace RVM.CineTrack.Domain.Entities;

/// <summary>
/// Tracks when a user watched a specific episode (checklist item).
/// </summary>
public class EpisodeWatch
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int EpisodeId { get; set; }
    public DateTime WatchedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public AppUser User { get; set; } = null!;
    public Episode Episode { get; set; } = null!;
}
