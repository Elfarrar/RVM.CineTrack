namespace RVM.CineTrack.Domain.Entities;

/// <summary>
/// Local user profile. Linked to AuthForge via ExternalId (OIDC sub claim).
/// </summary>
public class AppUser
{
    public int Id { get; set; }
    public string ExternalId { get; set; } = string.Empty; // AuthForge OIDC sub
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public List<WatchListItem> WatchList { get; set; } = [];
    public List<Review> Reviews { get; set; } = [];
    public List<EpisodeWatch> EpisodeWatches { get; set; } = [];
    public List<ReleaseAlert> ReleaseAlerts { get; set; } = [];
}
