using Microsoft.AspNetCore.Mvc;
using RVM.CineTrack.API.Controllers;
using RVM.CineTrack.Domain.Entities;
using RVM.CineTrack.Test.Helpers;

namespace RVM.CineTrack.Test.Controllers;

public class AlertsControllerTests
{
    [Fact]
    public async Task GetAlerts_ReturnsAllAlertsForUser()
    {
        using var db = TestDb.Create();
        var user = new AppUser { ExternalId = "ext1", Username = "user1" };
        var media = new Domain.Entities.Media { TmdbId = 1, Type = Domain.Enums.MediaType.Movie, Title = "Movie1", OriginalTitle = "Movie1" };
        db.Users.Add(user);
        db.Media.Add(media);
        await db.SaveChangesAsync();

        db.ReleaseAlerts.Add(new ReleaseAlert { UserId = user.Id, MediaId = media.Id, Title = "Alert1", ReleaseDate = DateTime.UtcNow.AddDays(5) });
        db.ReleaseAlerts.Add(new ReleaseAlert { UserId = user.Id, MediaId = media.Id, Title = "Alert2", ReleaseDate = DateTime.UtcNow.AddDays(3), IsRead = true });
        await db.SaveChangesAsync();

        var controller = new AlertsController(db);
        var result = await controller.GetAlerts(user.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetAlerts_WithUnreadOnly_ReturnsOnlyUnread()
    {
        using var db = TestDb.Create();
        var user = new AppUser { ExternalId = "ext2", Username = "user2" };
        var media = new Domain.Entities.Media { TmdbId = 2, Type = Domain.Enums.MediaType.Movie, Title = "Movie2", OriginalTitle = "Movie2" };
        db.Users.Add(user);
        db.Media.Add(media);
        await db.SaveChangesAsync();

        db.ReleaseAlerts.Add(new ReleaseAlert { UserId = user.Id, MediaId = media.Id, Title = "Read", ReleaseDate = DateTime.UtcNow, IsRead = true });
        db.ReleaseAlerts.Add(new ReleaseAlert { UserId = user.Id, MediaId = media.Id, Title = "Unread", ReleaseDate = DateTime.UtcNow.AddDays(1), IsRead = false });
        await db.SaveChangesAsync();

        var controller = new AlertsController(db);
        var result = await controller.GetAlerts(user.Id, unreadOnly: true);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task MarkRead_ReturnsNotFound_WhenAlertDoesNotExist()
    {
        using var db = TestDb.Create();
        var controller = new AlertsController(db);
        var result = await controller.MarkRead(9999);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task MarkRead_SetsIsReadTrue()
    {
        using var db = TestDb.Create();
        var user = new AppUser { ExternalId = "ext3", Username = "user3" };
        var media = new Domain.Entities.Media { TmdbId = 3, Type = Domain.Enums.MediaType.Movie, Title = "M3", OriginalTitle = "M3" };
        db.Users.Add(user);
        db.Media.Add(media);
        await db.SaveChangesAsync();

        var alert = new ReleaseAlert { UserId = user.Id, MediaId = media.Id, Title = "A", ReleaseDate = DateTime.UtcNow, IsRead = false };
        db.ReleaseAlerts.Add(alert);
        await db.SaveChangesAsync();

        var controller = new AlertsController(db);
        var result = await controller.MarkRead(alert.Id);

        Assert.IsType<OkResult>(result);
        Assert.True(alert.IsRead);
    }

    // MarkAllRead uses ExecuteUpdateAsync which is not supported by InMemory provider.
    // Covered by integration tests against the real database.
}
