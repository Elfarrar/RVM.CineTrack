using Microsoft.EntityFrameworkCore;
using RVM.CineTrack.Domain.Entities;
using RVM.CineTrack.Domain.Enums;
using RVM.CineTrack.Infrastructure.Data;
using RVM.CineTrack.Infrastructure.Services;

namespace RVM.CineTrack.API.Services;

public class MediaSyncService(CineTrackDbContext db, TmdbClient tmdb, ILogger<MediaSyncService> logger)
{
    public async Task<Media?> SyncMovieAsync(int tmdbId)
    {
        var existing = await db.Media.FirstOrDefaultAsync(m => m.TmdbId == tmdbId && m.Type == MediaType.Movie);
        var detail = await tmdb.GetMovieDetailAsync(tmdbId);
        if (detail is null) return existing;

        existing ??= new Media { TmdbId = tmdbId, Type = MediaType.Movie };

        existing.Title = detail.Title;
        existing.OriginalTitle = detail.OriginalTitle;
        existing.Overview = detail.Overview;
        existing.PosterPath = detail.PosterPath;
        existing.BackdropPath = detail.BackdropPath;
        existing.ReleaseDate = ParseDate(detail.ReleaseDate);
        existing.TmdbRating = detail.VoteAverage;
        existing.TmdbVoteCount = detail.VoteCount;
        existing.Runtime = detail.Runtime;
        existing.Genres = detail.Genres != null ? string.Join(", ", detail.Genres.Select(g => g.Name)) : null;
        existing.GenreIds = detail.Genres != null ? string.Join(",", detail.Genres.Select(g => g.Id)) : null;
        existing.Status = detail.Status;
        existing.LastSyncedAt = DateTime.UtcNow;

        if (existing.Id == 0)
            db.Media.Add(existing);

        await db.SaveChangesAsync();

        // Sync cast (top 15)
        if (detail.Credits?.Cast != null)
            await SyncCastAsync(existing.Id, detail.Credits);

        // Sync watch providers (BR)
        var providers = await tmdb.GetMovieWatchProvidersAsync(tmdbId);
        if (providers != null)
            await SyncWatchProvidersAsync(existing.Id, providers);

        return existing;
    }

    public async Task<Media?> SyncTvSeriesAsync(int tmdbId, bool syncEpisodes = false)
    {
        var existing = await db.Media
            .Include(m => m.Seasons)
            .FirstOrDefaultAsync(m => m.TmdbId == tmdbId && m.Type == MediaType.TvSeries);
        var detail = await tmdb.GetTvDetailAsync(tmdbId);
        if (detail is null) return existing;

        existing ??= new Media { TmdbId = tmdbId, Type = MediaType.TvSeries };

        existing.Title = detail.Name;
        existing.OriginalTitle = detail.OriginalName;
        existing.Overview = detail.Overview;
        existing.PosterPath = detail.PosterPath;
        existing.BackdropPath = detail.BackdropPath;
        existing.ReleaseDate = ParseDate(detail.FirstAirDate);
        existing.TmdbRating = detail.VoteAverage;
        existing.TmdbVoteCount = detail.VoteCount;
        existing.Runtime = detail.EpisodeRunTime?.FirstOrDefault();
        existing.Genres = detail.Genres != null ? string.Join(", ", detail.Genres.Select(g => g.Name)) : null;
        existing.GenreIds = detail.Genres != null ? string.Join(",", detail.Genres.Select(g => g.Id)) : null;
        existing.Status = detail.Status;
        existing.NumberOfSeasons = detail.NumberOfSeasons;
        existing.NumberOfEpisodes = detail.NumberOfEpisodes;
        existing.LastSyncedAt = DateTime.UtcNow;

        if (existing.Id == 0)
            db.Media.Add(existing);

        await db.SaveChangesAsync();

        // Sync seasons
        if (detail.Seasons != null)
            await SyncSeasonsAsync(existing, detail.Seasons, tmdbId, syncEpisodes);

        // Sync cast
        if (detail.Credits?.Cast != null)
            await SyncCastAsync(existing.Id, detail.Credits);

        // Sync watch providers
        var providers = await tmdb.GetTvWatchProvidersAsync(tmdbId);
        if (providers != null)
            await SyncWatchProvidersAsync(existing.Id, providers);

        return existing;
    }

    private async Task SyncSeasonsAsync(Media media, List<TmdbSeasonSummary> tmdbSeasons, int tmdbSeriesId, bool syncEpisodes)
    {
        foreach (var ts in tmdbSeasons.Where(s => s.SeasonNumber > 0)) // skip specials (season 0)
        {
            var season = media.Seasons.FirstOrDefault(s => s.SeasonNumber == ts.SeasonNumber);
            if (season is null)
            {
                season = new Season { MediaId = media.Id, SeasonNumber = ts.SeasonNumber };
                db.Seasons.Add(season);
            }

            season.TmdbSeasonId = ts.Id;
            season.Name = ts.Name;
            season.Overview = ts.Overview;
            season.PosterPath = ts.PosterPath;
            season.AirDate = ParseDate(ts.AirDate);
            season.EpisodeCount = ts.EpisodeCount;

            await db.SaveChangesAsync();

            if (syncEpisodes)
                await SyncEpisodesAsync(season, tmdbSeriesId);
        }
    }

    private async Task SyncEpisodesAsync(Season season, int tmdbSeriesId)
    {
        var detail = await tmdb.GetSeasonDetailAsync(tmdbSeriesId, season.SeasonNumber);
        if (detail?.Episodes is null) return;

        var existingEpisodes = await db.Episodes
            .Where(e => e.SeasonId == season.Id)
            .ToListAsync();

        foreach (var te in detail.Episodes)
        {
            var episode = existingEpisodes.FirstOrDefault(e => e.EpisodeNumber == te.EpisodeNumber);
            if (episode is null)
            {
                episode = new Episode { SeasonId = season.Id, EpisodeNumber = te.EpisodeNumber };
                db.Episodes.Add(episode);
            }

            episode.TmdbEpisodeId = te.Id;
            episode.Name = te.Name;
            episode.Overview = te.Overview;
            episode.StillPath = te.StillPath;
            episode.AirDate = ParseDate(te.AirDate);
            episode.Runtime = te.Runtime;
        }

        await db.SaveChangesAsync();
    }

    private async Task SyncCastAsync(int mediaId, TmdbCredits credits)
    {
        var existing = await db.MediaCast.Where(mc => mc.MediaId == mediaId).ToListAsync();
        db.MediaCast.RemoveRange(existing);

        var castToAdd = (credits.Cast ?? [])
            .Take(15)
            .Select(c => new MediaCast
            {
                MediaId = mediaId,
                TmdbPersonId = c.Id,
                Name = c.Name,
                Character = c.Character,
                ProfilePath = c.ProfilePath,
                Department = "Acting",
                Order = c.Order
            }).ToList();

        // Add top directors
        var directors = (credits.Crew ?? [])
            .Where(c => c.Job == "Director")
            .Take(3)
            .Select(c => new MediaCast
            {
                MediaId = mediaId,
                TmdbPersonId = c.Id,
                Name = c.Name,
                ProfilePath = c.ProfilePath,
                Department = "Directing",
                Order = 0
            });

        castToAdd.AddRange(directors);
        db.MediaCast.AddRange(castToAdd);
        await db.SaveChangesAsync();
    }

    private async Task SyncWatchProvidersAsync(int mediaId, TmdbWatchProviderResponse response)
    {
        var existing = await db.WatchProviders.Where(wp => wp.MediaId == mediaId).ToListAsync();
        db.WatchProviders.RemoveRange(existing);

        if (response.Results is null || !response.Results.TryGetValue("BR", out var br))
        {
            await db.SaveChangesAsync();
            return;
        }

        var providers = new List<WatchProvider>();
        var link = br.Link;

        void AddProviders(List<TmdbProvider>? list, string type)
        {
            if (list is null) return;
            foreach (var p in list)
            {
                if (providers.Any(x => x.TmdbProviderId == p.ProviderId && x.Type == type))
                    continue;
                providers.Add(new WatchProvider
                {
                    MediaId = mediaId,
                    TmdbProviderId = p.ProviderId,
                    Name = p.ProviderName,
                    LogoPath = p.LogoPath,
                    Link = link,
                    Type = type,
                    LastSyncedAt = DateTime.UtcNow
                });
            }
        }

        AddProviders(br.Flatrate, "flatrate");
        AddProviders(br.Rent, "rent");
        AddProviders(br.Buy, "buy");

        db.WatchProviders.AddRange(providers);
        await db.SaveChangesAsync();
    }

    private static DateTime? ParseDate(string? date) =>
        DateTime.TryParse(date, out var d) ? d : null;
}
