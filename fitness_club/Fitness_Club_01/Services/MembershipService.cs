using Fitness_Club_01.Data;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Services;

public static class MembershipService
{
    public static async Task ExpireOutdatedAsync(ApplicationDbContext db)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var active = await db.MembershipStatuses.FirstAsync(s => s.StatusCode == "active");
        var expired = await db.MembershipStatuses.FirstAsync(s => s.StatusCode == "expired");

        var outdated = await db.Memberships
            .Where(m => m.IdMembershipStatus == active.IdMembershipStatus && m.EndDate < today)
            .ToListAsync();

        if (outdated.Count == 0)
            return;

        foreach (var membership in outdated)
        {
            membership.IdMembershipStatus = expired.IdMembershipStatus;
            db.MembershipStatusHistories.Add(new MembershipStatusHistory
            {
                IdMembership = membership.IdMembership,
                IdMembershipStatus = expired.IdMembershipStatus,
                ChangedAt = DateTimeDb.Now,
                Comment = "Автоматическое истечение срока"
            });
        }

        await db.SaveChangesAsync();
    }

    public static async Task<Membership?> GetActiveMembershipAsync(ApplicationDbContext db, int idClient) =>
        await GetMembershipForDateAsync(db, idClient, DateOnly.FromDateTime(DateTime.Today));

    public static async Task<Membership?> GetMembershipForDateAsync(
        ApplicationDbContext db,
        int idClient,
        DateOnly date)
    {
        await ExpireOutdatedAsync(db);

        var active = await db.MembershipStatuses.FirstAsync(s => s.StatusCode == "active");

        return await db.Memberships
            .Include(m => m.IdMembershipTypeNavigation)
            .Where(m => m.IdClient == idClient
                        && m.IdMembershipStatus == active.IdMembershipStatus
                        && m.StartDate <= date
                        && m.EndDate >= date)
            .OrderByDescending(m => m.EndDate)
            .FirstOrDefaultAsync();
    }

    public static async Task<bool> HasActiveMembershipAsync(ApplicationDbContext db, int idClient) =>
        await GetActiveMembershipAsync(db, idClient) != null;

    public static string GetEffectiveStatusName(Membership membership, string statusCode, string statusName)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        if (statusCode == "active" && membership.StartDate > today)
            return "Ожидает начала";

        if (statusCode == "active" && membership.StartDate <= today && membership.EndDate >= today)
            return statusName;

        return statusName;
    }

    public static async Task<string?> ValidateMembershipForDateAsync(
        ApplicationDbContext db,
        int idClient,
        DateOnly date,
        bool forClient = false)
    {
        if (await GetMembershipForDateAsync(db, idClient, date) != null)
            return null;

        var subject = forClient ? "У вас" : "У клиента";
        var active = await db.MembershipStatuses.FirstAsync(s => s.StatusCode == "active");
        var today = DateOnly.FromDateTime(DateTime.Today);

        var pending = await db.Memberships
            .Where(m => m.IdClient == idClient
                        && m.IdMembershipStatus == active.IdMembershipStatus
                        && m.StartDate > today)
            .OrderBy(m => m.StartDate)
            .FirstOrDefaultAsync();

        if (pending != null)
        {
            if (date < pending.StartDate)
                return $"{subject} абонемент начнёт действовать с {pending.StartDate:dd.MM.yyyy}";

            return null;
        }

        var expiredOnDate = await db.Memberships
            .AnyAsync(m => m.IdClient == idClient
                           && m.IdMembershipStatus == active.IdMembershipStatus
                           && m.EndDate < date);

        if (expiredOnDate)
            return $"{subject} нет действующего абонемента на {date:dd.MM.yyyy}";

        return $"{subject} нет действующего абонемента";
    }

    public static async Task<string?> ChangeStatusAsync(
        ApplicationDbContext db,
        int idMembership,
        string statusCode,
        string comment,
        int? changedByUserId)
    {
        await ExpireOutdatedAsync(db);

        var membership = await db.Memberships.FindAsync(idMembership);
        if (membership == null)
            return "Абонемент не найден";

        var newStatus = await db.MembershipStatuses.FirstOrDefaultAsync(s => s.StatusCode == statusCode);
        if (newStatus == null)
            return "Статус не найден";

        if (membership.IdMembershipStatus == newStatus.IdMembershipStatus)
            return "Абонемент уже в этом статусе";

        var active = await db.MembershipStatuses.FirstAsync(s => s.StatusCode == "active");
        if (statusCode == "active")
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var duplicate = await db.Memberships.AnyAsync(m =>
                m.IdClient == membership.IdClient
                && m.IdMembership != membership.IdMembership
                && m.IdMembershipStatus == active.IdMembershipStatus
                && m.StartDate <= today
                && m.EndDate >= today);

            if (duplicate)
                return "У клиента уже есть другой активный абонемент";
        }

        membership.IdMembershipStatus = newStatus.IdMembershipStatus;
        db.MembershipStatusHistories.Add(new MembershipStatusHistory
        {
            IdMembership = membership.IdMembership,
            IdMembershipStatus = newStatus.IdMembershipStatus,
            ChangedAt = DateTimeDb.Now,
            IdChangedByUser = changedByUserId,
            Comment = comment
        });

        await db.SaveChangesAsync();
        return null;
    }
}
