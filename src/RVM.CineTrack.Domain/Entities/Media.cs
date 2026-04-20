namespace RVM.CineTrack.Domain.Entities;

using RVM.CineTrack.Domain.Enums;

/// <summary>
/// Cached media item from TMDB (movie or TV series).
/// Only stored when a user searches/adds it.
/// </summary>
public class Media
{
    public int Id { get; set; }
    public int TmdbId { get; set; }
    public MediaType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OriginalTitle { get; set; } = string.Empty;
    public string? Overview { get; set; }
    public string? PosterPath { get; set; }
    public string? BackdropPath { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public double TmdbRating { get; set; }
    public int TmdbVoteCount { get; set; }
    public int? Runtime { get; set; } // minutes (movies) or avg episode runtime (TV)
    public string? Genres { get; set; } // comma-separated genre names
    public string? GenreIds { get; set; } // comma-separated TMDB genre IDs
    public string? Status { get; set; } // Ended, Returning Series, Released, etc.
    public int? NumberOfSeasons { get; set; }
    public int? NumberOfEpisodes { get; set; }
    public DateTime LastSyncedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public List<WatchListItem> WatchListItems { get; set; } = [];
    public List<Review> Reviews { get; set; } = [];
    public List<Season> Seasons { get; set; } = [];
    public List<MediaCast> Cast { get; set; } = [];
    public List<WatchProvider> WatchProviders { get; set; } = [];
}
