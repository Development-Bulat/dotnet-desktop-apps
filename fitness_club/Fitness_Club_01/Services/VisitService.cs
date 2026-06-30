using Fitness_Club_01.Data;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Services;

public static class VisitService
{
    public static async Task<(int Current, int Capacity)> GetClubOccupancyAsync(ApplicationDbContext db)
    {
        var capacity = await db.GymHalls.SumAsync(h => (int?)h.Capacity) ?? 0;
        if (capacity <= 0)
            return (0, 0);

        var now = DateTimeDb.Now;
        var since = now.AddHours(-ScheduleValidator.ConcurrentVisitHours);
        var current = await db.Visits.CountAsync(v => v.VisitDateTime >= since && v.VisitDateTime <= now);
        return (current, capacity);
    }

    public static async Task<string?> MarkVisitAsync(
        ApplicationDbContext db,
        int idClient,
        DateOnly visitDate)
    {
        if (!Session.IsReception)
            return "Отмечать визиты может только сотрудник ресепшн";

        if (Session.CurrentUser == null)
            return "Сессия недействительна";

        var today = DateOnly.FromDateTime(DateTime.Today);
        if (visitDate != today)
            return "Визит можно отметить только за сегодня";

        var membership = await MembershipService.GetActiveMembershipAsync(db, idClient);
        if (membership == null)
            return "Нет действующего абонемента на сегодня";

        if (membership.IdMembershipTypeNavigation.VisitLimit.HasValue)
        {
            var used = await db.Visits.CountAsync(v => v.IdMembership == membership.IdMembership);
            if (used >= membership.IdMembershipTypeNavigation.VisitLimit.Value)
                return "Лимит посещений исчерпан";
        }

        var alreadyToday = await db.Visits.AnyAsync(v =>
            v.IdClient == idClient
            && DateOnly.FromDateTime(v.VisitDateTime) == visitDate);

        if (alreadyToday)
            return "Визит за сегодня уже отмечен";

        var clubCapacityError = await ScheduleValidator.ValidateClubVisitCapacityAsync(db);
        if (clubCapacityError != null)
            return clubCapacityError;

        db.Visits.Add(new Visit
        {
            IdClient = idClient,
            VisitDateTime = DateTimeDb.Now,
            IdMarkedByUser = Session.CurrentUser.IdUserAccount,
            IdMembership = membership.IdMembership
        });

        await db.SaveChangesAsync();
        return null;
    }
}
