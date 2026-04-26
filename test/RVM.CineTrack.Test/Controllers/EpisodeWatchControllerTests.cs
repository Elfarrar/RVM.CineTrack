using Microsoft.AspNetCore.Mvc;
using RVM.CineTrack.API.Controllers;
using RVM.CineTrack.Domain.Entities;
using RVM.CineTrack.Domain.Enums;
using RVM.CineTrack.Test.Helpers;

namespace RVM.CineTrack.Test.Controllers;

public class EpisodeWatchControllerTests
{
    private static async Task<(AppUser user, Episode episode, CineTrack.Infrastructure.Data.CineTrackDbContext db)> SetupAsync()
    {
        var db = TestDb.Create();
        var user = new AppUser { ExternalId = $"ext-{Guid.NewGuid()}", Username = $"u-{Guid.NewGuid():N}" };
        var media = new Domain.Entities.Media { TmdbId = 200, Type = MediaType.TvSeries, Title = "Show", OriginalTitle = "Show" };
        db.Users.Add(user);
        db.Media.Add(media);
        await db.SaveChangesAsync();

        var season = new Season { MediaId = media.Id, TmdbSeasonId = 10, SeasonNumber = 1, Name = "S1", EpisodeCount = 5 };
        db.Seasons.Add(season);
        await db.SaveChangesAsync();

        var episode = new Episode { SeasonId = season.Id, TmdbEpisodeId = 101, EpisodeNumber = 1, Name = "Pilot" };
        db.Episodes.Add(episode);
        await db.SaveChangesAsync();
        return (user, episode, db);
    }

    [Fact]
    public async Task MarkWatched_ReturnsNotFound_WhenEpisodeDoesNotExist()
    {
        using var db = TestDb.Create();
        var controller = new EpisodeWatchController(db);
        var result = await controller.MarkWatched(1, 9999);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task MarkWatched_ReturnsCreated_ForNewWatch()
    {
        var (user, episode, db) = await SetupAsync();
        using (db)
        {
            var controller = new EpisodeWatchController(db);
            var result = await controller.MarkWatched(user.Id, episode.Id);
            Assert.IsType<CreatedResult>(result);
        }
    }

    [Fact]
    public async Task MarkWatched_ReturnsOk_WhenAlreadyWatched()
    {
        var (user, episode, db) = await SetupAsync();
        using (db)
        {
            db.EpisodeWatches.Add(new EpisodeWatch { UserId = user.Id, EpisodeId = episode.Id });
            await db.SaveChangesAsync();

            var controller = new EpisodeWatchController(db);
            var result = await controller.MarkWatched(user.Id, episode.Id);
            Assert.IsType<OkObjectResult>(result);
        }
    }

    [Fact]
    public async Task UnmarkWatched_ReturnsNotFound_WhenNoWatchRecord()
    {
        using var db = TestDb.Create();
        var controller = new EpisodeWatchController(db);
        var result = await controller.UnmarkWatched(1, 9999);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UnmarkWatched_ReturnsNoContent_WhenWatchExists()
    {
        var (user, episode, db) = await SetupAsync();
        using (db)
        {
            db.EpisodeWatches.Add(new EpisodeWatch { UserId = user.Id, EpisodeId = episode.Id });
            await db.SaveChangesAsync();

            var controller = new EpisodeWatchController(db);
            var result = await controller.UnmarkWatched(user.Id, episode.Id);
            Assert.IsType<NoContentResult>(result);
        }
    }

    [Fact]
    public async Task GetWatched_ReturnsOk()
    {
        var (user, episode, db) = await SetupAsync();
        using (db)
        {
            db.EpisodeWatches.Add(new EpisodeWatch { UserId = user.Id, EpisodeId = episode.Id });
            await db.SaveChangesAsync();

            var controller = new EpisodeWatchController(db);
            var result = await controller.GetWatched(user.Id, null);
            Assert.IsType<OkObjectResult>(result);
        }
    }

    [Fact]
    public async Task MarkSeasonWatched_MarksAllEpisodes()
    {
        var (user, episode, db) = await SetupAsync();
        using (db)
        {
            var ep2 = new Episode { SeasonId = episode.SeasonId, TmdbEpisodeId = 102, EpisodeNumber = 2, Name = "Ep2" };
            db.Episodes.Add(ep2);
            await db.SaveChangesAsync();

            var controller = new EpisodeWatchController(db);
            var result = await controller.MarkSeasonWatched(user.Id, episode.SeasonId);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }
    }
}
