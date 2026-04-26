using Microsoft.Extensions.Logging.Abstractions;
using RVM.CineTrack.API.Services;
using RVM.CineTrack.Domain.Enums;
using RVM.CineTrack.Infrastructure.Services;
using RVM.CineTrack.Test.Helpers;

namespace RVM.CineTrack.Test.Services;

// Configurable fake that returns specified TMDB results
file class TmdbFake : FakeTmdbClient
{
    private TmdbMovieDetail? _movieDetail;
    private TmdbTvDetail? _tvDetail;
    private TmdbWatchProviderResponse? _providers;

    public TmdbFake WithMovieDetail(TmdbMovieDetail? detail) { _movieDetail = detail; return this; }
    public TmdbFake WithTvDetail(TmdbTvDetail? detail) { _tvDetail = detail; return this; }
    public TmdbFake WithProviders(TmdbWatchProviderResponse? p) { _providers = p; return this; }

    public override Task<TmdbMovieDetail?> GetMovieDetailAsync(int tmdbId, string language = "pt-BR")
        => Task.FromResult(_movieDetail);
    public override Task<TmdbTvDetail?> GetTvDetailAsync(int tmdbId, string language = "pt-BR")
        => Task.FromResult(_tvDetail);
    public override Task<TmdbWatchProviderResponse?> GetMovieWatchProvidersAsync(int tmdbId)
        => Task.FromResult(_providers);
    public override Task<TmdbWatchProviderResponse?> GetTvWatchProvidersAsync(int tmdbId)
        => Task.FromResult(_providers);
}

public class MediaSyncServiceTests
{
    private static TmdbMovieDetail MakeMovieDetail(int id = 1, string title = "Movie") =>
        new(id, title, title, "Overview", "/poster.jpg", null, "2024-01-01", 7.5, 1000, 120,
            [new TmdbGenre(28, "Action"), new TmdbGenre(12, "Adventure")],
            "Released", new TmdbCredits(
                [new TmdbCastMember(100, "Actor1", "Hero", null, 0)],
                [new TmdbCrewMember(200, "Director1", "Directing", "Director", null)]));

    private static TmdbTvDetail MakeTvDetail(int id = 10, string name = "Show") =>
        new(id, name, name, "Overview", "/poster.jpg", null, "2020-01-01", 8.0, 5000,
            [45], [new TmdbGenre(18, "Drama")], "Returning Series", 3, 30,
            [new TmdbSeasonSummary(1001, 1, "Season 1", null, null, "2020-01-01", 10),
             new TmdbSeasonSummary(1002, 2, "Season 2", null, null, "2021-01-01", 10)],
            new TmdbCredits([], []));

    [Fact]
    public async Task SyncMovieAsync_CreatesNewMedia_WhenNotInDb()
    {
        using var db = TestDb.Create();
        var tmdb = new TmdbFake().WithMovieDetail(MakeMovieDetail());

        var service = new MediaSyncService(db, tmdb, NullLogger<MediaSyncService>.Instance);
        var result = await service.SyncMovieAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Movie", result.Title);
        Assert.Equal(MediaType.Movie, result.Type);
        Assert.Equal(1, result.TmdbId);
    }

    [Fact]
    public async Task SyncMovieAsync_ReturnsExisting_WhenTmdbFails()
    {
        using var db = TestDb.Create();
        var existing = new Domain.Entities.Media { TmdbId = 2, Type = MediaType.Movie, Title = "Old Movie", OriginalTitle = "Old Movie" };
        db.Media.Add(existing);
        await db.SaveChangesAsync();

        var tmdb = new TmdbFake().WithMovieDetail(null); // TMDB fails

        var service = new MediaSyncService(db, tmdb, NullLogger<MediaSyncService>.Instance);
        var result = await service.SyncMovieAsync(2);

        Assert.NotNull(result);
        Assert.Equal("Old Movie", result.Title);
    }

    [Fact]
    public async Task SyncMovieAsync_ReturnsNull_WhenNotInDbAndTmdbFails()
    {
        using var db = TestDb.Create();
        var tmdb = new TmdbFake().WithMovieDetail(null);

        var service = new MediaSyncService(db, tmdb, NullLogger<MediaSyncService>.Instance);
        var result = await service.SyncMovieAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task SyncMovieAsync_UpdatesExisting_WhenAlreadyInDb()
    {
        using var db = TestDb.Create();
        var existing = new Domain.Entities.Media { TmdbId = 3, Type = MediaType.Movie, Title = "Old Title", OriginalTitle = "Old" };
        db.Media.Add(existing);
        await db.SaveChangesAsync();

        var tmdb = new TmdbFake().WithMovieDetail(MakeMovieDetail(3, "New Title"));

        var service = new MediaSyncService(db, tmdb, NullLogger<MediaSyncService>.Instance);
        var result = await service.SyncMovieAsync(3);

        Assert.NotNull(result);
        Assert.Equal("New Title", result.Title);
    }

    [Fact]
    public async Task SyncTvSeriesAsync_CreatesNewMedia_WithSeasons()
    {
        using var db = TestDb.Create();
        var tmdb = new TmdbFake().WithTvDetail(MakeTvDetail());

        var service = new MediaSyncService(db, tmdb, NullLogger<MediaSyncService>.Instance);
        var result = await service.SyncTvSeriesAsync(10, syncEpisodes: false);

        Assert.NotNull(result);
        Assert.Equal("Show", result.Title);
        Assert.Equal(MediaType.TvSeries, result.Type);
        Assert.Equal(3, result.NumberOfSeasons);

        var seasons = db.Seasons.Where(s => s.MediaId == result.Id).ToList();
        Assert.Equal(2, seasons.Count);
    }

    [Fact]
    public async Task SyncTvSeriesAsync_ReturnsNull_WhenNotInDbAndTmdbFails()
    {
        using var db = TestDb.Create();
        var tmdb = new TmdbFake().WithTvDetail(null);

        var service = new MediaSyncService(db, tmdb, NullLogger<MediaSyncService>.Instance);
        var result = await service.SyncTvSeriesAsync(999, syncEpisodes: false);

        Assert.Null(result);
    }

    [Fact]
    public async Task SyncMovieAsync_SyncsGenresCorrectly()
    {
        using var db = TestDb.Create();
        var tmdb = new TmdbFake().WithMovieDetail(MakeMovieDetail(4, "Genre Movie"));

        var service = new MediaSyncService(db, tmdb, NullLogger<MediaSyncService>.Instance);
        var result = await service.SyncMovieAsync(4);

        Assert.NotNull(result);
        Assert.Contains("Action", result.Genres);
        Assert.Contains("Adventure", result.Genres);
    }

    [Fact]
    public async Task SyncMovieAsync_SyncsWatchProviders_WhenBrDataExists()
    {
        using var db = TestDb.Create();
        var providers = new TmdbWatchProviderResponse(new Dictionary<string, TmdbCountryProviders>
        {
            ["BR"] = new TmdbCountryProviders("https://justwatch.com",
                [new TmdbProvider(8, "Netflix", "/netflix.jpg")], null, null)
        });

        var tmdb = new TmdbFake().WithMovieDetail(MakeMovieDetail(5, "Streamed Movie")).WithProviders(providers);

        var service = new MediaSyncService(db, tmdb, NullLogger<MediaSyncService>.Instance);
        var result = await service.SyncMovieAsync(5);

        Assert.NotNull(result);
        var watchProviders = db.WatchProviders.Where(wp => wp.MediaId == result.Id).ToList();
        Assert.Single(watchProviders);
        Assert.Equal("Netflix", watchProviders[0].Name);
    }
}
