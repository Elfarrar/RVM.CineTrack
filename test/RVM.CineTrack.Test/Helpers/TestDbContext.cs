using Microsoft.EntityFrameworkCore;
using RVM.CineTrack.Infrastructure.Data;

namespace RVM.CineTrack.Test.Helpers;

public static class TestDb
{
    public static CineTrackDbContext Create()
    {
        var options = new DbContextOptionsBuilder<CineTrackDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new CineTrackDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }
}
