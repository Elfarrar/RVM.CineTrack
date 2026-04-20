namespace RVM.CineTrack.Domain.Entities;

/// <summary>
/// Cast/crew member linked to a media. Cached from TMDB credits.
/// Used for "most watched artists" stats.
/// </summary>
public class MediaCast
{
    public int Id { get; set; }
    public int MediaId { get; set; }
    public int TmdbPersonId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Character { get; set; }
    public string? ProfilePath { get; set; }
    public string Department { get; set; } = "Acting"; // Acting, Directing, etc.
    public int Order { get; set; }

    // Navigation
    public Media Media { get; set; } = null!;
}
