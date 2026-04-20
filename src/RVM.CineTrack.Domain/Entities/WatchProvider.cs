namespace RVM.CineTrack.Domain.Entities;

/// <summary>
/// Streaming provider for a media in Brazil. Cached from TMDB watch/providers.
/// </summary>
public class WatchProvider
{
    public int Id { get; set; }
    public int MediaId { get; set; }
    public int TmdbProviderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
    public string? Link { get; set; }
    public string Type { get; set; } = "flatrate"; // flatrate, rent, buy
    public DateTime LastSyncedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Media Media { get; set; } = null!;
}
