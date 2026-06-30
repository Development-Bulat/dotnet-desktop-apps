using Fitness_Club_01.Data;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Services;

public static class DbRefresh
{
    public static void ClearCache(ApplicationDbContext db)
    {
        foreach (var entry in db.ChangeTracker.Entries().ToList())
            entry.State = EntityState.Detached;
    }
}
