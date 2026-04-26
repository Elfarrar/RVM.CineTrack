using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RVM.CineTrack.API.Workers;
using RVM.CineTrack.Domain.Entities;
using RVM.CineTrack.Domain.Enums;
using RVM.CineTrack.Infrastructure.Services;
using RVM.CineTrack.Test.Helpers;

namespace RVM.CineTrack.Test.Workers;

public class ReleaseAlertWorkerTests
{
    private static IServiceScopeFactory CreateScopeFactory(Infrastructure.Data.CineTrackDbContext db)
    {
        var services = new ServiceCollection();
        services.AddSingleton(db);
        services.AddSingleton<TmdbClient>(new FakeTmdbClient());
        var provider = services.BuildServiceProvider();

        var mockFactory = new Mock<IServiceScopeFactory>();
        var mockScope = new Mock<IServiceScope>();
        mockScope.Setup(s => s.ServiceProvider).Returns(provider);
        mockFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);
        return mockFactory.Object;
    }

    private static Task InvokeCheckReleasesAsync(ReleaseAlertWorker worker)
    {
        var method = typeof(ReleaseAlertWorker).GetMethod("CheckReleasesAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        return (Task)method.Invoke(worker, [CancellationToken.None])!;
    }

    [Fact]
    public async Task CheckReleases_CreatesAlert_ForUpcomingTvSeason()
    {
        using var db = TestDb.Create();

        var user = new AppUser { ExternalId = "worker-ext-1", Username = "wuser1" };
        var media = new Domain.Entities.Media { TmdbId = 1001, Type = MediaType.TvSeries, Title = "Show1", OriginalTitle = "Show1" };
        db.Users.Add(user);
        db.Media.Add(media);
        await db.SaveChangesAsync();

        db.WatchListItems.Add(new WatchListItem { UserId = user.Id, MediaId = media.Id, Status = WatchStatus.Watching });
        await db.SaveChangesAsync();

        var season = new Season
        {
            MediaId = media.Id,
            TmdbSeasonId = 30,
            SeasonNumber = 2,
            Name = "Season 2",
            EpisodeCount = 8,
            AirDate = DateTime.UtcNow.AddDays(5) // within 7-day window
        };
        db.Seasons.Add(season);
        await db.SaveChangesAsync();

        var worker = new ReleaseAlertWorker(CreateScopeFactory(db), NullLogger<ReleaseAlertWorker>.Instance);
        await InvokeCheckReleasesAsync(worker);

        var alerts = await db.ReleaseAlerts.ToListAsync();
        Assert.Single(alerts);
        Assert.Equal(user.Id, alerts[0].UserId);
        Assert.Equal(media.Id, alerts[0].MediaId);
    }

    [Fact]
    public async Task CheckReleases_CreatesAlert_ForUpcomingMovie()
    {
        using var db = TestDb.Create();

        var user = new AppUser { ExternalId = "worker-ext-2", Username = "wuser2" };
        var media = new Domain.Entities.Media
        {
            TmdbId = 1002, Type = MediaType.Movie, Title = "Upcoming Movie", OriginalTitle = "Upcoming Movie",
            ReleaseDate = DateTime.UtcNow.AddDays(3)
        };
        db.Users.Add(user);
        db.Media.Add(media);
        await db.SaveChangesAsync();

        db.WatchListItems.Add(new WatchListItem { UserId = user.Id, MediaId = media.Id, Status = WatchStatus.WantToWatch });
        await db.SaveChangesAsync();

        var worker = new ReleaseAlertWorker(CreateScopeFactory(db), NullLogger<ReleaseAlertWorker>.Instance);
        await InvokeCheckReleasesAsync(worker);

        var alerts = await db.ReleaseAlerts.ToListAsync();
        Assert.Single(alerts);
        Assert.Equal(media.Id, alerts[0].MediaId);
    }

    [Fact]
    public async Task CheckReleases_DoesNotDuplicate_WhenAlertAlreadyExists()
    {
        using var db = TestDb.Create();

        var user = new AppUser { ExternalId = "worker-ext-3", Username = "wuser3" };
        var media = new Domain.Entities.Media
        {
            TmdbId = 1003, Type = MediaType.Movie, Title = "Movie3", OriginalTitle = "Movie3",
            ReleaseDate = DateTime.UtcNow.AddDays(2)
        };
        db.Users.Add(user);
        db.Media.Add(media);
        await db.SaveChangesAsync();

        db.WatchListItems.Add(new WatchListItem { UserId = user.Id, MediaId = media.Id, Status = WatchStatus.WantToWatch });
        // Pre-existing alert
        db.ReleaseAlerts.Add(new ReleaseAlert
        {
            UserId = user.Id, MediaId = media.Id,
            Title = "Movie3", ReleaseDate = media.ReleaseDate!.Value
        });
        await db.SaveChangesAsync();

        var worker = new ReleaseAlertWorker(CreateScopeFactory(db), NullLogger<ReleaseAlertWorker>.Instance);
        await InvokeCheckReleasesAsync(worker);

        var alerts = await db.ReleaseAlerts.ToListAsync();
        Assert.Single(alerts); // Still only 1 - no duplicate
    }

    [Fact]
    public async Task CheckReleases_IgnoresMovies_OutsideWindow()
    {
        using var db = TestDb.Create();

        var user = new AppUser { ExternalId = "worker-ext-4", Username = "wuser4" };
        var media = new Domain.Entities.Media
        {
            TmdbId = 1004, Type = MediaType.Movie, Title = "FarFuture", OriginalTitle = "FarFuture",
            ReleaseDate = DateTime.UtcNow.AddDays(30) // outside 7-day window
        };
        db.Users.Add(user);
        db.Media.Add(media);
        await db.SaveChangesAsync();

        db.WatchListItems.Add(new WatchListItem { UserId = user.Id, MediaId = media.Id, Status = WatchStatus.WantToWatch });
        await db.SaveChangesAsync();

        var worker = new ReleaseAlertWorker(CreateScopeFactory(db), NullLogger<ReleaseAlertWorker>.Instance);
        await InvokeCheckReleasesAsync(worker);

        var alerts = await db.ReleaseAlerts.ToListAsync();
        Assert.Empty(alerts);
    }
}
