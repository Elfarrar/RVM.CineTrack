using Microsoft.AspNetCore.Mvc;
using RVM.CineTrack.API.Controllers;
using RVM.CineTrack.Domain.Entities;
using RVM.CineTrack.Test.Helpers;

namespace RVM.CineTrack.Test.Controllers;

public class UsersControllerTests
{
    [Fact]
    public async Task Get_ReturnsNotFound_WhenUserDoesNotExist()
    {
        using var db = TestDb.Create();
        var controller = new UsersController(db);
        var result = await controller.Get(9999);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Get_ReturnsUser_WhenExists()
    {
        using var db = TestDb.Create();
        var user = new AppUser { ExternalId = "sub-1", Username = "alice" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var controller = new UsersController(db);
        var result = await controller.Get(user.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(user, ok.Value);
    }

    [Fact]
    public async Task GetByExternalId_ReturnsNotFound_WhenNotFound()
    {
        using var db = TestDb.Create();
        var controller = new UsersController(db);
        var result = await controller.GetByExternalId("nonexistent");
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetByExternalId_ReturnsUser_WhenExists()
    {
        using var db = TestDb.Create();
        var user = new AppUser { ExternalId = "sub-ext-42", Username = "bob" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var controller = new UsersController(db);
        var result = await controller.GetByExternalId("sub-ext-42");

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(user, ok.Value);
    }

    [Fact]
    public async Task Create_CreatesUser_WhenExternalIdIsNew()
    {
        using var db = TestDb.Create();
        var controller = new UsersController(db);
        var request = new UsersController.CreateUserRequest("new-ext-id", "charlie", "Charlie Brown", null);

        var result = await controller.Create(request);

        var created = Assert.IsType<CreatedResult>(result);
        Assert.NotNull(created.Value);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenExternalIdAlreadyExists()
    {
        using var db = TestDb.Create();
        var user = new AppUser { ExternalId = "dup-ext", Username = "dave" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var controller = new UsersController(db);
        var request = new UsersController.CreateUserRequest("dup-ext", "dave2", null, null);

        var result = await controller.Create(request);
        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task RecentActivity_ReturnsOk()
    {
        using var db = TestDb.Create();
        var controller = new UsersController(db);
        var result = await controller.RecentActivity(5);
        Assert.IsType<OkObjectResult>(result);
    }
}
