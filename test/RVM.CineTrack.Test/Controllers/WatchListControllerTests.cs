using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RVM.CineTrack.API.Controllers;
using RVM.CineTrack.API.Services;
using RVM.CineTrack.Domain.Entities;
using RVM.CineTrack.Domain.Enums;
using RVM.CineTrack.Infrastructure.Data;
using RVM.CineTrack.Infrastructure.Services;
using RVM.CineTrack.Test.Helpers;

namespace RVM.CineTrack.Test.Controllers;

public class WatchListControllerTests
{
    private static MediaSyncService CreateSyncMock(CineTrackDbContext db, Domain.Entities.Media? returnMedia = null)
    {
        var mock = new Mock<MediaSyncService>(db, new FakeTmdbClient(), NullLogger<MediaSyncService>.Instance)
        { CallBase = false };

        mock.Setup(s => s.SyncMovieAsync(It.IsAny<int>())).ReturnsAsync(returnMedia);
        mock.Setup(s => s.SyncTvSeriesAsync(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(returnMedia);

        return mock.Object;
    }

    [Fact]
    public async Task GetUserWatchList_ReturnsOk()
    {
        using var db = TestDb.Create();
        var user = new AppUser { ExternalId = "ext-wl1", Username = "wluser1" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var sync = CreateSyncMock(db);
        var controller = new WatchListController(db, sync);
        var result = await controller.GetUserWatchList(user.Id, null);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetUserWatchList_FiltersByStatus()
    {
        using var db = TestDb.Create();
        var user = new AppUser { ExternalId = "ext-wl2", Username = "wluser2" };
        var media = new Domain.Entities.Media { TmdbId = 300, Type = MediaType.Movie, Title = "M", OriginalTitle = "M" };
        db.Users.Add(user);
        db.Media.Add(media);
        await db.SaveChangesAsync();

        db.WatchListItems.Add(new WatchListItem { UserId = user.Id, MediaId = media.Id, Status = WatchStatus.Watched });
        await db.SaveChangesAsync();

        var sync = CreateSyncMock(db);
        var controller = new WatchListController(db, sync);
        var result = await controller.GetUserWatchList(user.Id, WatchStatus.WantToWatch);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task Add_ReturnsConflict_WhenMediaAlreadyInList()
    {
        using var db = TestDb.Create();
        var user = new AppUser { ExternalId = "ext-wl3", Username = "wluser3" };
        var media = new Domain.Entities.Media { TmdbId = 400, Type = MediaType.Movie, Title = "M2", OriginalTitle = "M2" };
        db.Users.Add(user);
        db.Media.Add(media);
        await db.SaveChangesAsync();

        db.WatchListItems.Add(new WatchListItem { UserId = user.Id, MediaId = media.Id, Status = WatchStatus.Watching });
        await db.SaveChangesAsync();

        // Sync mock returns the existing media (already in db)
        var sync = CreateSyncMock(db, media);
        var controller = new WatchListController(db, sync);
        var request = new WatchListController.AddToWatchListRequest(media.TmdbId, "movie");
        var result = await controller.Add(request, user.Id);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Add_Returns502_WhenSyncFails()
    {
        using var db = TestDb.Create();
        var user = new AppUser { ExternalId = "ext-wl4", Username = "wluser4" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var sync = CreateSyncMock(db, null); // sync returns null = TMDB failed
        var controller = new WatchListController(db, sync);
        var request = new WatchListController.AddToWatchListRequest(9999, "movie");
        var result = await controller.Add(request, user.Id);

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(502, status.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_ReturnsNotFound_WhenItemMissing()
    {
        using var db = TestDb.Create();
        var sync = CreateSyncMock(db);
        var controller = new WatchListController(db, sync);
        var result = await controller.UpdateStatus(9999, new WatchListController.UpdateStatusRequest(WatchStatus.Watched, null));
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateStatus_UpdatesStatusCorrectly()
    {
        using var db = TestDb.Create();
        var user = new AppUser { ExternalId = "ext-wl5", Username = "wluser5" };
        var media = new Domain.Entities.Media { TmdbId = 500, Type = MediaType.Movie, Title = "M5", OriginalTitle = "M5" };
        db.Users.Add(user);
        db.Media.Add(media);
        await db.SaveChangesAsync();

        var item = new WatchListItem { UserId = user.Id, MediaId = media.Id, Status = WatchStatus.WantToWatch };
        db.WatchListItems.Add(item);
        await db.SaveChangesAsync();

        var sync = CreateSyncMock(db);
        var controller = new WatchListController(db, sync);
        var result = await controller.UpdateStatus(item.Id, new WatchListController.UpdateStatusRequest(WatchStatus.Watched, 5));

        var ok = Assert.IsType<OkObjectResult>(result);
        var updated = Assert.IsType<WatchListItem>(ok.Value);
        Assert.Equal(WatchStatus.Watched, updated.Status);
        Assert.Equal(5, updated.Rating);
        Assert.NotNull(updated.FinishedAt);
    }

    [Fact]
    public async Task Remove_ReturnsNotFound_WhenItemMissing()
    {
        using var db = TestDb.Create();
        var sync = CreateSyncMock(db);
        var controller = new WatchListController(db, sync);
        var result = await controller.Remove(9999);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Remove_ReturnsNoContent_WhenItemExists()
    {
        using var db = TestDb.Create();
        var user = new AppUser { ExternalId = "ext-wl6", Username = "wluser6" };
        var media = new Domain.Entities.Media { TmdbId = 600, Type = MediaType.Movie, Title = "M6", OriginalTitle = "M6" };
        db.Users.Add(user);
        db.Media.Add(media);
        await db.SaveChangesAsync();

        var item = new WatchListItem { UserId = user.Id, MediaId = media.Id, Status = WatchStatus.WantToWatch };
        db.WatchListItems.Add(item);
        await db.SaveChangesAsync();

        var sync = CreateSyncMock(db);
        var controller = new WatchListController(db, sync);
        var result = await controller.Remove(item.Id);
        Assert.IsType<NoContentResult>(result);
    }
}
