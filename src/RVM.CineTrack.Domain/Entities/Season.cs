namespace RVM.CineTrack.Domain.Entities;

/// <summary>
/// TV series season, synced from TMDB.
/// </summary>
public class Season
{
    public int Id { get; set; }
    public int MediaId { get; set; }
    public int TmdbSeasonId { get; set; }
    public int SeasonNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Overview { get; set; }
    public string? PosterPath { get; set; }
    public DateTime? AirDate { get; set; }
    public int EpisodeCount { get; set; }

    // Navigation
    public Media Media { get; set; } = null!;
    public List<Episode> Episodes { get; set; } = [];
    public List<Review> Reviews { get; set; } = [];
}
