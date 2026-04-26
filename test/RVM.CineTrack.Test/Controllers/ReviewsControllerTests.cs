using Microsoft.AspNetCore.Mvc;
using RVM.CineTrack.API.Controllers;
using RVM.CineTrack.Domain.Entities;
using RVM.CineTrack.Domain.Enums;
using RVM.CineTrack.Test.Helpers;

namespace RVM.CineTrack.Test.Controllers;

public class ReviewsControllerTests
{
    private static async Task<(Domain.Entities.Media media, AppUser user, CineTrack.Infrastructure.Data.CineTrackDbContext db)> SetupAsync()
    {
        var db = TestDb.Create();
        var user = new AppUser { ExternalId = $"ext-{Guid.NewGuid()}", Username = $"user-{Guid.NewGuid():N}" };
        var media = new Domain.Entities.Media { TmdbId = 100, Type = MediaType.Movie, Title = "Test Movie", OriginalTitle = "Test Movie" };
        db.Users.Add(user);
        db.Media.Add(media);
        await db.SaveChangesAsync();
        return (media, user, db);
    }

    [Fact]
    public async Task Create_ReturnsCreated_ForNewReview()
    {
        var (media, user, db) = await SetupAsync();
        using (db)
        {
            var controller = new ReviewsController(db);
            var request = new ReviewsController.CreateReviewRequest(user.Id, 4, "Great!", media.Id, null, null);
            var result = await controller.Create(request);
            Assert.IsType<CreatedResult>(result);
        }
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenRatingOutOfRange()
    {
        var (media, user, db) = await SetupAsync();
        using (db)
        {
            var controller = new ReviewsController(db);
            var request = new ReviewsController.CreateReviewRequest(user.Id, 0, "Invalid", media.Id, null, null);
            var result = await controller.Create(request);
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenNoTargetSpecified()
    {
        var (_, user, db) = await SetupAsync();
        using (db)
        {
            var controller = new ReviewsController(db);
            var request = new ReviewsController.CreateReviewRequest(user.Id, 3, "?", null, null, null);
            var result = await controller.Create(request);
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }

    [Fact]
    public async Task Create_UpdatesExistingReview_WhenDuplicate()
    {
        var (media, user, db) = await SetupAsync();
        using (db)
        {
            var controller = new ReviewsController(db);
            var req1 = new ReviewsController.CreateReviewRequest(user.Id, 3, "Ok", media.Id, null, null);
            await controller.Create(req1);

            var req2 = new ReviewsController.CreateReviewRequest(user.Id, 5, "Excellent!", media.Id, null, null);
            var result = await controller.Create(req2);

            var ok = Assert.IsType<OkObjectResult>(result);
            var updated = Assert.IsType<Review>(ok.Value);
            Assert.Equal(5, updated.Rating);
        }
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenReviewMissing()
    {
        using var db = TestDb.Create();
        var controller = new ReviewsController(db);
        var result = await controller.Delete(9999);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenReviewExists()
    {
        var (media, user, db) = await SetupAsync();
        using (db)
        {
            var review = new Review { UserId = user.Id, MediaId = media.Id, Rating = 4 };
            db.Reviews.Add(review);
            await db.SaveChangesAsync();

            var controller = new ReviewsController(db);
            var result = await controller.Delete(review.Id);
            Assert.IsType<NoContentResult>(result);
        }
    }

    [Fact]
    public async Task GetMediaReviews_ReturnsOk()
    {
        var (media, user, db) = await SetupAsync();
        using (db)
        {
            db.Reviews.Add(new Review { UserId = user.Id, MediaId = media.Id, Rating = 3 });
            await db.SaveChangesAsync();

            var controller = new ReviewsController(db);
            var result = await controller.GetMediaReviews(media.Id);
            Assert.IsType<OkObjectResult>(result);
        }
    }

    [Fact]
    public async Task TopRated_ReturnsOk()
    {
        using var db = TestDb.Create();
        var controller = new ReviewsController(db);
        var result = await controller.TopRated(10);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task WorstRated_ReturnsOk()
    {
        using var db = TestDb.Create();
        var controller = new ReviewsController(db);
        var result = await controller.WorstRated(10);
        Assert.IsType<OkObjectResult>(result);
    }
}
