using Microsoft.Extensions.Logging.Abstractions;
using RVM.CineTrack.Infrastructure.Services;

namespace RVM.CineTrack.Test.Helpers;

/// <summary>
/// Fake TmdbClient that returns null for all calls.
/// Used as a stand-in when the real TMDB HTTP client is not needed.
/// </summary>
public class FakeTmdbClient() : TmdbClient(new HttpClient(), NullLogger<TmdbClient>.Instance)
{
    public override Task<TmdbSearchResult<TmdbMultiSearchItem>?> SearchMultiAsync(string query, int page = 1, string language = "pt-BR")
        => Task.FromResult<TmdbSearchResult<TmdbMultiSearchItem>?>(null);

    public override Task<TmdbMovieDetail?> GetMovieDetailAsync(int tmdbId, string language = "pt-BR")
        => Task.FromResult<TmdbMovieDetail?>(null);

    public override Task<TmdbWatchProviderResponse?> GetMovieWatchProvidersAsync(int tmdbId)
        => Task.FromResult<TmdbWatchProviderResponse?>(null);

    public override Task<TmdbTvDetail?> GetTvDetailAsync(int tmdbId, string language = "pt-BR")
        => Task.FromResult<TmdbTvDetail?>(null);

    public override Task<TmdbSeasonDetail?> GetSeasonDetailAsync(int tmdbId, int seasonNumber, string language = "pt-BR")
        => Task.FromResult<TmdbSeasonDetail?>(null);

    public override Task<TmdbWatchProviderResponse?> GetTvWatchProvidersAsync(int tmdbId)
        => Task.FromResult<TmdbWatchProviderResponse?>(null);

    public override Task<TmdbSearchResult<TmdbMultiSearchItem>?> GetTrendingAsync(string timeWindow = "week", string language = "pt-BR")
        => Task.FromResult<TmdbSearchResult<TmdbMultiSearchItem>?>(null);

    public override Task<TmdbUpcomingResponse?> GetUpcomingMoviesAsync(string language = "pt-BR", string region = "BR")
        => Task.FromResult<TmdbUpcomingResponse?>(null);

    public override Task<TmdbSearchResult<TmdbMultiSearchItem>?> GetOnTheAirTvAsync(string language = "pt-BR")
        => Task.FromResult<TmdbSearchResult<TmdbMultiSearchItem>?>(null);
}
