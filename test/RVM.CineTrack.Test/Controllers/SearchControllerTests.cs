using Microsoft.AspNetCore.Mvc;
using RVM.CineTrack.API.Controllers;
using RVM.CineTrack.Infrastructure.Services;
using RVM.CineTrack.Test.Helpers;

namespace RVM.CineTrack.Test.Controllers;

// Custom fake that returns configurable results
file class ConfigurableTmdbClient(
    TmdbSearchResult<TmdbMultiSearchItem>? searchResult = null,
    TmdbSearchResult<TmdbMultiSearchItem>? trendingResult = null,
    TmdbUpcomingResponse? upcomingResult = null,
    TmdbSearchResult<TmdbMultiSearchItem>? onAirResult = null) : FakeTmdbClient
{
    public override Task<TmdbSearchResult<TmdbMultiSearchItem>?> SearchMultiAsync(string query, int page = 1, string language = "pt-BR")
        => Task.FromResult(searchResult);

    public override Task<TmdbSearchResult<TmdbMultiSearchItem>?> GetTrendingAsync(string timeWindow = "week", string language = "pt-BR")
        => Task.FromResult(trendingResult);

    public override Task<TmdbUpcomingResponse?> GetUpcomingMoviesAsync(string language = "pt-BR", string region = "BR")
        => Task.FromResult(upcomingResult);

    public override Task<TmdbSearchResult<TmdbMultiSearchItem>?> GetOnTheAirTvAsync(string language = "pt-BR")
        => Task.FromResult(onAirResult);
}

public class SearchControllerTests
{
    private static TmdbMultiSearchItem MakeItem(int id, string mediaType, string? title = null, string? name = null) =>
        new(id, mediaType, title, name, null, null, null, null, null, null, null, 7.0, 100, null);

    [Fact]
    public async Task Search_ReturnsBadRequest_WhenQueryIsEmpty()
    {
        var controller = new SearchController(new FakeTmdbClient());
        var result = await controller.Search("", 1);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Search_ReturnsBadRequest_WhenQueryIsWhitespace()
    {
        var controller = new SearchController(new FakeTmdbClient());
        var result = await controller.Search("   ", 1);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Search_Returns502_WhenTmdbUnavailable()
    {
        var controller = new SearchController(new ConfigurableTmdbClient(searchResult: null));
        var result = await controller.Search("matrix", 1);
        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(502, status.StatusCode);
    }

    [Fact]
    public async Task Search_ReturnsOk_WithFilteredResults()
    {
        var searchResult = new TmdbSearchResult<TmdbMultiSearchItem>(
            1, [MakeItem(1, "movie", "The Matrix"), MakeItem(2, "person", null, "Keanu Reeves")], 1, 2);

        var controller = new SearchController(new ConfigurableTmdbClient(searchResult: searchResult));
        var result = await controller.Search("matrix", 1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Trending_Returns502_WhenTmdbUnavailable()
    {
        var controller = new SearchController(new ConfigurableTmdbClient(trendingResult: null));
        var result = await controller.Trending("week");
        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(502, status.StatusCode);
    }

    [Fact]
    public async Task Trending_ReturnsOk_WithFilteredResults()
    {
        var trendingResult = new TmdbSearchResult<TmdbMultiSearchItem>(
            1, [MakeItem(10, "tv", null, "Breaking Bad")], 1, 1);

        var controller = new SearchController(new ConfigurableTmdbClient(trendingResult: trendingResult));
        var result = await controller.Trending("week");
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Upcoming_ReturnsOk_WithCombinedResults()
    {
        var upcoming = new TmdbUpcomingResponse(
            [MakeItem(1, "movie", "New Film")], null);
        var onAir = new TmdbSearchResult<TmdbMultiSearchItem>(
            1, [MakeItem(2, "tv", null, "New Show")], 1, 1);

        var controller = new SearchController(new ConfigurableTmdbClient(upcomingResult: upcoming, onAirResult: onAir));
        var result = await controller.Upcoming();
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Upcoming_ReturnsOk_WhenBothAreNull()
    {
        var controller = new SearchController(new ConfigurableTmdbClient(upcomingResult: null, onAirResult: null));
        var result = await controller.Upcoming();
        Assert.IsType<OkObjectResult>(result);
    }
}
