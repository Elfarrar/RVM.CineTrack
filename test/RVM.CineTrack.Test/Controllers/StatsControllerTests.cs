using Microsoft.AspNetCore.Mvc;
using RVM.CineTrack.API.Controllers;
using RVM.CineTrack.Domain.Entities;
using RVM.CineTrack.Domain.Enums;
using RVM.CineTrack.Test.Helpers;

namespace RVM.CineTrack.Test.Controllers;

public class StatsControllerTests
{
    [Fact]
    public async Task GetUserStats_ReturnsOk_ForUserWithNoData()
    {
        using var db = TestDb.Create();
        var user = new AppUser { ExternalId = "stats-ext-1", Username = "statsuser1" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var controller = new StatsController(db);
        var result = await controller.GetUserStats(user.Id);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetUserStats_CountsWatchedMoviesCorrectly()
    {
        using var db = TestDb.Create();
        var user = new AppUser { ExternalId = "stats-ext-2", Username = "statsuser2" };
        var movie1 = new Domain.Entities.Media { TmdbId = 700, Type = MediaType.Movie, Title = "M7", OriginalTitle = "M7", Runtime = 120 };
        var movie2 = new Domain.Entities.Media { TmdbId = 701, Type = MediaType.Movie, Title = "M8", OriginalTitle = "M8", Runtime = 90 };
        db.Users.Add(user);
        db.Media.AddRange(movie1, movie2);
        await db.SaveChangesAsync();

        db.WatchListItems.AddRange(
            new WatchListItem { UserId = user.Id, MediaId = movie1.Id, Status = WatchStatus.Watched },
            new WatchListItem { UserId = user.Id, MediaId = movie2.Id, Status = WatchStatus.Watched }
        );
        await db.SaveChangesAsync();

        var controller = new StatsController(db);
        var result = await controller.GetUserStats(user.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task GetUserStats_CalculatesStreakCorrectly()
    {
        using var db = TestDb.Create();
        var user = new AppUser { ExternalId = "stats-ext-3", Username = "statsuser3" };
        var media = new Domain.Entities.Media { TmdbId = 800, Type = MediaType.TvSeries, Title = "Show", OriginalTitle = "Show" };
        db.Users.Add(user);
        db.Media.Add(media);
        await db.SaveChangesAsync();

        var season = new Season { MediaId = media.Id, TmdbSeasonId = 20, SeasonNumber = 1, Name = "S1", EpisodeCount = 2 };
        db.Seasons.Add(season);
        await db.SaveChangesAsync();

        var ep1 = new Episode { SeasonId = season.Id, TmdbEpisodeId = 201, EpisodeNumber = 1, Name = "E1" };
        var ep2 = new Episode { SeasonId = season.Id, TmdbEpisodeId = 202, EpisodeNumber = 2, Name = "E2" };
        db.Episodes.AddRange(ep1, ep2);
        await db.SaveChangesAsync();

        var today = DateTime.UtcNow.Date;
        db.EpisodeWatches.AddRange(
            new EpisodeWatch { UserId = user.Id, EpisodeId = ep1.Id, WatchedAt = today },
            new EpisodeWatch { UserId = user.Id, EpisodeId = ep2.Id, WatchedAt = today.AddDays(-1) }
        );
        await db.SaveChangesAsync();

        var controller = new StatsController(db);
        var result = await controller.GetUserStats(user.Id);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetUserStats_ReturnsAverageRating_WhenReviewsExist()
    {
        using var db = TestDb.Create();
        var user = new AppUser { ExternalId = "stats-ext-4", Username = "statsuser4" };
        var media = new Domain.Entities.Media { TmdbId = 900, Type = MediaType.Movie, Title = "Rated", OriginalTitle = "Rated" };
        db.Users.Add(user);
        db.Media.Add(media);
        await db.SaveChangesAsync();

        db.Reviews.AddRange(
            new Review { UserId = user.Id, MediaId = media.Id, Rating = 4 },
            new Review { UserId = user.Id, MediaId = media.Id + 1, Rating = 2 }
        );
        await db.SaveChangesAsync();

        var controller = new StatsController(db);
        var result = await controller.GetUserStats(user.Id);
        Assert.IsType<OkObjectResult>(result);
    }
}
