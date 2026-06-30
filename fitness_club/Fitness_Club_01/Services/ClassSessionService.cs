using Fitness_Club_01.Data;
using Fitness_Club_01.Models;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Services;

public static class ClassSessionService
{
    public static async Task<List<ClassSessionRow>> GetMonthlySessionsAsync(
        ApplicationDbContext db,
        int trainerId,
        int year,
        int month)
    {
        var firstDay = new DateOnly(year, month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);
        var today = DateOnly.FromDateTime(DateTime.Today);

        var classes = await db.GroupClasses
            .Include(g => g.IdDayOfWeekNavigation)
            .Include(g => g.IdGymHallNavigation)
            .AsNoTracking()
            .Where(g => g.IdTrainer == trainerId && g.IsActive)
            .ToListAsync();

        var cancelled = await db.CancelledClassSessions
            .AsNoTracking()
            .Where(c => c.ClassDate >= firstDay && c.ClassDate <= lastDay)
            .Select(c => new { c.IdGroupClass, c.ClassDate })
            .ToListAsync();

        var cancelledSet = cancelled
            .Select(c => (c.IdGroupClass, c.ClassDate))
            .ToHashSet();

        var bookingCounts = await db.ClassBookings
            .AsNoTracking()
            .Where(b => b.ClassDate >= firstDay && b.ClassDate <= lastDay)
            .GroupBy(b => new { b.IdGroupClass, b.ClassDate })
            .Select(g => new { g.Key.IdGroupClass, g.Key.ClassDate, Count = g.Count() })
            .ToListAsync();

        var countMap = bookingCounts.ToDictionary(
            x => (x.IdGroupClass, x.ClassDate),
            x => x.Count);

        var bookingClients = await db.ClassBookings
            .AsNoTracking()
            .Where(b => b.ClassDate >= firstDay && b.ClassDate <= lastDay)
            .Select(b => new { b.IdGroupClass, b.ClassDate, b.IdClient })
            .ToListAsync();

        var bookingMap = bookingClients
            .GroupBy(b => (b.IdGroupClass, b.ClassDate))
            .ToDictionary(g => g.Key, g => g.Select(x => x.IdClient).ToHashSet());

        var visitStart = firstDay.ToDateTime(TimeOnly.MinValue);
        var visitEnd = lastDay.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var visits = await db.Visits
            .AsNoTracking()
            .Where(v => v.VisitDateTime >= visitStart && v.VisitDateTime < visitEnd)
            .Select(v => new { v.VisitDateTime, v.IdClient })
            .ToListAsync();

        var visitSet = visits
            .Select(v => (DateOnly.FromDateTime(v.VisitDateTime), v.IdClient))
            .ToHashSet();

        var rows = new List<ClassSessionRow>();
        var number = 1;

        for (var date = firstDay; date <= lastDay; date = date.AddDays(1))
        {
            var dayNumber = ScheduleValidator.DayNumberFromDate(date);
            foreach (var groupClass in classes.Where(g => g.IdDayOfWeekNavigation.DayNumber == dayNumber))
            {
                var isCancelled = cancelledSet.Contains((groupClass.IdGroupClass, date));
                var booked = countMap.GetValueOrDefault((groupClass.IdGroupClass, date), 0);

                rows.Add(new ClassSessionRow
                {
                    IdGroupClass = groupClass.IdGroupClass,
                    Number = number++,
                    ClassDateValue = date,
                    ClassDate = date.ToString("dd.MM.yyyy"),
                    ClassTime = groupClass.StartTime.ToString("HH:mm"),
                    ClassName = groupClass.ClassName,
                    HallName = groupClass.IdGymHallNavigation.HallName,
                    BookedCount = $"{booked}/{groupClass.MaxParticipants}",
                    Status = ResolveSessionStatus(
                        isCancelled,
                        date,
                        today,
                        groupClass.IdGroupClass,
                        booked,
                        bookingMap,
                        visitSet),
                    IsCancelled = isCancelled,
                    CanCancel = !isCancelled && date >= today
                });
            }
        }

        return rows
            .OrderBy(r => r.ClassDateValue)
            .ThenBy(r => r.ClassTime)
            .Select((row, index) =>
            {
                row.Number = index + 1;
                return row;
            })
            .ToList();
    }

    public static async Task<string?> CancelSessionAsync(
        ApplicationDbContext db,
        int trainerId,
        int idGroupClass,
        DateOnly classDate,
        UserAccount cancelledBy)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (classDate < today)
            return "Нельзя отменить прошедшее занятие";

        var groupClass = await db.GroupClasses
            .Include(g => g.IdTrainerNavigation)
            .Include(g => g.IdDayOfWeekNavigation)
            .FirstOrDefaultAsync(g => g.IdGroupClass == idGroupClass);

        if (groupClass == null)
            return "Занятие не найдено";

        if (groupClass.IdTrainer != trainerId)
            return "Можно отменить только свои занятия";

        var dayError = BookingValidator.ValidateClassDate(groupClass, classDate);
        if (dayError != null)
            return dayError;

        var alreadyCancelled = await db.CancelledClassSessions.AnyAsync(c =>
            c.IdGroupClass == idGroupClass && c.ClassDate == classDate);

        if (alreadyCancelled)
            return "Занятие уже отменено";

        var bookings = await db.ClassBookings
            .Include(b => b.IdClientNavigation)
            .Include(b => b.IdGroupClassNavigation)
                .ThenInclude(g => g.IdTrainerNavigation)
            .Where(b => b.IdGroupClass == idGroupClass && b.ClassDate == classDate)
            .ToListAsync();

        db.CancelledClassSessions.Add(new CancelledClassSession
        {
            IdGroupClass = idGroupClass,
            ClassDate = classDate,
            CancelledAt = DateTimeDb.Now,
            IdCancelledByUser = cancelledBy.IdUserAccount
        });

        if (bookings.Count > 0)
            await NotificationService.NotifyClassSessionCancelledAsync(db, groupClass, classDate, bookings);

        db.ClassBookings.RemoveRange(bookings);
        await db.SaveChangesAsync();
        return null;
    }

    public static async Task<bool> IsSessionCancelledAsync(
        ApplicationDbContext db,
        int idGroupClass,
        DateOnly classDate)
    {
        return await db.CancelledClassSessions.AnyAsync(c =>
            c.IdGroupClass == idGroupClass && c.ClassDate == classDate);
    }

    private static string ResolveSessionStatus(
        bool isCancelled,
        DateOnly date,
        DateOnly today,
        int idGroupClass,
        int bookedCount,
        Dictionary<(int IdGroupClass, DateOnly ClassDate), HashSet<int>> bookingMap,
        HashSet<(DateOnly Date, int IdClient)> visitSet)
    {
        if (isCancelled)
            return "Отменено";

        if (date >= today)
            return "Запланировано";

        if (bookedCount == 0)
            return "Занятие не состоялось";

        if (!bookingMap.TryGetValue((idGroupClass, date), out var clients) || clients.Count == 0)
            return "Занятие не состоялось";

        return clients.Any(idClient => visitSet.Contains((date, idClient)))
            ? "Проведено"
            : "Занятие не состоялось";
    }
}
