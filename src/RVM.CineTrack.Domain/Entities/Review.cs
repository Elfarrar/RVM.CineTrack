namespace RVM.CineTrack.Domain.Entities;

/// <summary>
/// Public review with rating (1-5 oscars) and comment.
/// Can target a Media, Season, or Episode.
/// </summary>
public class Review
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? MediaId { get; set; }
    public int? SeasonId { get; set; }
    public int? EpisodeId { get; set; }
    public int Rating { get; set; } // 1-5 oscars
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public AppUser User { get; set; } = null!;
    public Media? Media { get; set; }
    public Season? Season { get; set; }
    public Episode? Episode { get; set; }
}
