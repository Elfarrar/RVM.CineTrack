using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RVM.CineTrack.API.Controllers;
using RVM.CineTrack.API.Services;
using RVM.CineTrack.Domain.Entities;
using RVM.CineTrack.Domain.Enums;
using RVM.CineTrack.Infrastructure.Data;
using RVM.CineTrack.Test.Helpers;

namespace RVM.CineTrack.Test.Controllers;

public class MediaControllerTests
{
    private static MediaSyncService CreateSyncMock(CineTrackDbContext db, Domain.Entities.Media? movieResult = null, Domain.Entities.Media? tvResult = null)
    {
        var mock = new Mock<MediaSyncService>(db,
            new FakeTmdbClient(),
            NullLogger<MediaSyncService>.Instance)
        { CallBase = false };

        mock.Setup(s => s.SyncMovieAsync(It.IsAny<int>())).ReturnsAsync(movieResult);
        mock.Setup(s => s.SyncTvSeriesAsync(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(tvResult);

        return mock.Object;
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenMediaDoesNotExist()
    {
        using var db = TestDb.Create();
        var sync = CreateSyncMock(db);
        var controller = new MediaController(db, sync);
        var result = await controller.Get(9999);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Get_ReturnsMedia_WhenExists()
    {
        using var db = TestDb.Create();
        var media = new Domain.Entities.Media { TmdbId = 1, Type = MediaType.Movie, Title = "Inception", OriginalTitle = "Inception" };
        db.Media.Add(media);
        await db.SaveChangesAsync();

        var sync = CreateSyncMock(db);
        var controller = new MediaController(db, sync);
        var result = await controller.Get(media.Id);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task SyncMovie_Returns502_WhenSyncFails()
    {
        using var db = TestDb.Create();
        var sync = CreateSyncMock(db, movieResult: null);
        var controller = new MediaController(db, sync);
        var result = await controller.SyncMovie(99999);

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(502, status.StatusCode);
    }

    [Fact]
    public async Task SyncMovie_ReturnsOk_WhenSyncSucceeds()
    {
        using var db = TestDb.Create();
        var media = new Domain.Entities.Media { TmdbId = 123, Type = MediaType.Movie, Title = "Dune", OriginalTitle = "Dune" };
        db.Media.Add(media);
        await db.SaveChangesAsync();

        var sync = CreateSyncMock(db, movieResult: media);
        var controller = new MediaController(db, sync);
        var result = await controller.SyncMovie(123);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task SyncTv_Returns502_WhenSyncFails()
    {
        using var db = TestDb.Create();
        var sync = CreateSyncMock(db, tvResult: null);
        var controller = new MediaController(db, sync);
        var result = await controller.SyncTv(99999, false);

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(502, status.StatusCode);
    }

    [Fact]
    public async Task SyncTv_ReturnsOk_WhenSyncSucceeds()
    {
        using var db = TestDb.Create();
        var media = new Domain.Entities.Media { TmdbId = 456, Type = MediaType.TvSeries, Title = "Breaking Bad", OriginalTitle = "Breaking Bad" };
        db.Media.Add(media);
        await db.SaveChangesAsync();

        var sync = CreateSyncMock(db, tvResult: media);
        var controller = new MediaController(db, sync);
        var result = await controller.SyncTv(456, false);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetEpisodes_ReturnsNotFound_WhenSeasonMissing()
    {
        using var db = TestDb.Create();
        var sync = CreateSyncMock(db);
        var controller = new MediaController(db, sync);
        var result = await controller.GetEpisodes(9999, 1);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetProviders_ReturnsOk_WhenNoProviders()
    {
        using var db = TestDb.Create();
        var sync = CreateSyncMock(db);
        var controller = new MediaController(db, sync);
        var result = await controller.GetProviders(9999);
        Assert.IsType<OkObjectResult>(result);
    }
}
