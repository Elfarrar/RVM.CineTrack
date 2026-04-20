using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVM.CineTrack.Infrastructure.Services;

namespace RVM.CineTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController(TmdbClient tmdb) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int page = 1)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { error = "Query parameter 'q' is required." });

        var results = await tmdb.SearchMultiAsync(q, page);
        if (results is null)
            return StatusCode(502, new { error = "TMDB API unavailable." });

        // Filter to only movies and TV
        var filtered = results.Results
            .Where(r => r.MediaType is "movie" or "tv")
            .Select(r => new
            {
                tmdbId = r.Id,
                mediaType = r.MediaType,
                title = r.Title ?? r.Name,
                originalTitle = r.OriginalTitle ?? r.OriginalName,
                overview = r.Overview,
                posterPath = r.PosterPath,
                releaseDate = r.ReleaseDate ?? r.FirstAirDate,
                rating = r.VoteAverage,
                votes = r.VoteCount
            });

        return Ok(new
        {
            page = results.Page,
            totalPages = results.TotalPages,
            totalResults = results.TotalResults,
            results = filtered
        });
    }

    [HttpGet("trending")]
    public async Task<IActionResult> Trending([FromQuery] string window = "week")
    {
        var results = await tmdb.GetTrendingAsync(window);
        if (results is null)
            return StatusCode(502, new { error = "TMDB API unavailable." });

        var filtered = results.Results
            .Where(r => r.MediaType is "movie" or "tv")
            .Select(r => new
            {
                tmdbId = r.Id,
                mediaType = r.MediaType,
                title = r.Title ?? r.Name,
                posterPath = r.PosterPath,
                rating = r.VoteAverage
            });

        return Ok(new { results = filtered });
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> Upcoming()
    {
        var movies = await tmdb.GetUpcomingMoviesAsync();
        var tvShows = await tmdb.GetOnTheAirTvAsync();

        var combined = new List<object>();

        if (movies?.Results != null)
            combined.AddRange(movies.Results.Select(r => new
            {
                tmdbId = r.Id, mediaType = "movie", title = r.Title,
                posterPath = r.PosterPath, releaseDate = r.ReleaseDate
            }));

        if (tvShows?.Results != null)
            combined.AddRange(tvShows.Results.Select(r => new
            {
                tmdbId = r.Id, mediaType = "tv", title = r.Name,
                posterPath = r.PosterPath, releaseDate = r.FirstAirDate
            }));

        return Ok(new { results = combined.Take(40) });
    }
}
