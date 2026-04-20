namespace RVM.CineTrack.Infrastructure.Services;

using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

public class TmdbClient
{
    private readonly HttpClient _http;
    private readonly ILogger<TmdbClient> _logger;
    private const string BaseUrl = "https://api.themoviedb.org/3";

    public TmdbClient(HttpClient http, ILogger<TmdbClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    // --- Search ---

    public async Task<TmdbSearchResult<TmdbMultiSearchItem>?> SearchMultiAsync(string query, int page = 1, string language = "pt-BR")
    {
        var url = $"{BaseUrl}/search/multi?query={Uri.EscapeDataString(query)}&page={page}&language={language}&include_adult=false";
        return await GetAsync<TmdbSearchResult<TmdbMultiSearchItem>>(url);
    }

    // --- Movie ---

    public async Task<TmdbMovieDetail?> GetMovieDetailAsync(int tmdbId, string language = "pt-BR")
    {
        var url = $"{BaseUrl}/movie/{tmdbId}?language={language}&append_to_response=credits";
        return await GetAsync<TmdbMovieDetail>(url);
    }

    public async Task<TmdbWatchProviderResponse?> GetMovieWatchProvidersAsync(int tmdbId)
    {
        var url = $"{BaseUrl}/movie/{tmdbId}/watch/providers";
        return await GetAsync<TmdbWatchProviderResponse>(url);
    }

    // --- TV ---

    public async Task<TmdbTvDetail?> GetTvDetailAsync(int tmdbId, string language = "pt-BR")
    {
        var url = $"{BaseUrl}/tv/{tmdbId}?language={language}&append_to_response=credits";
        return await GetAsync<TmdbTvDetail>(url);
    }

    public async Task<TmdbSeasonDetail?> GetSeasonDetailAsync(int tmdbId, int seasonNumber, string language = "pt-BR")
    {
        var url = $"{BaseUrl}/tv/{tmdbId}/season/{seasonNumber}?language={language}";
        return await GetAsync<TmdbSeasonDetail>(url);
    }

    public async Task<TmdbWatchProviderResponse?> GetTvWatchProvidersAsync(int tmdbId)
    {
        var url = $"{BaseUrl}/tv/{tmdbId}/watch/providers";
        return await GetAsync<TmdbWatchProviderResponse>(url);
    }

    // --- Discover / Trending ---

    public async Task<TmdbSearchResult<TmdbMultiSearchItem>?> GetTrendingAsync(string timeWindow = "week", string language = "pt-BR")
    {
        var url = $"{BaseUrl}/trending/all/{timeWindow}?language={language}";
        return await GetAsync<TmdbSearchResult<TmdbMultiSearchItem>>(url);
    }

    public async Task<TmdbUpcomingResponse?> GetUpcomingMoviesAsync(string language = "pt-BR", string region = "BR")
    {
        var url = $"{BaseUrl}/movie/upcoming?language={language}&region={region}";
        return await GetAsync<TmdbUpcomingResponse>(url);
    }

    public async Task<TmdbSearchResult<TmdbMultiSearchItem>?> GetOnTheAirTvAsync(string language = "pt-BR")
    {
        var url = $"{BaseUrl}/tv/on_the_air?language={language}";
        return await GetAsync<TmdbSearchResult<TmdbMultiSearchItem>>(url);
    }

    // --- Genres ---

    public async Task<TmdbGenreList?> GetMovieGenresAsync(string language = "pt-BR")
    {
        var url = $"{BaseUrl}/genre/movie/list?language={language}";
        return await GetAsync<TmdbGenreList>(url);
    }

    public async Task<TmdbGenreList?> GetTvGenresAsync(string language = "pt-BR")
    {
        var url = $"{BaseUrl}/genre/tv/list?language={language}";
        return await GetAsync<TmdbGenreList>(url);
    }

    // --- Recommendations ---

    public async Task<TmdbSearchResult<TmdbMultiSearchItem>?> GetMovieRecommendationsAsync(int tmdbId, string language = "pt-BR")
    {
        var url = $"{BaseUrl}/movie/{tmdbId}/recommendations?language={language}";
        return await GetAsync<TmdbSearchResult<TmdbMultiSearchItem>>(url);
    }

    public async Task<TmdbSearchResult<TmdbMultiSearchItem>?> GetTvRecommendationsAsync(int tmdbId, string language = "pt-BR")
    {
        var url = $"{BaseUrl}/tv/{tmdbId}/recommendations?language={language}";
        return await GetAsync<TmdbSearchResult<TmdbMultiSearchItem>>(url);
    }

    // --- Helpers ---

    private async Task<T?> GetAsync<T>(string url)
    {
        try
        {
            return await _http.GetFromJsonAsync<T>(url);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "TMDB API request failed: {Url}", url);
            return default;
        }
    }
}
