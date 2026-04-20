namespace RVM.CineTrack.Domain.Entities;

/// <summary>
/// TV episode, synced from TMDB. Used for per-episode tracking.
/// </summary>
public class Episode
{
    public int Id { get; set; }
    public int SeasonId { get; set; }
    public int TmdbEpisodeId { get; set; }
    public int EpisodeNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Overview { get; set; }
    public string? StillPath { get; set; }
    public DateTime? AirDate { get; set; }
    public int? Runtime { get; set; } // minutes

    // Navigation
    public Season Season { get; set; } = null!;
    public List<EpisodeWatch> Watches { get; set; } = [];
    public List<Review> Reviews { get; set; } = [];
}
