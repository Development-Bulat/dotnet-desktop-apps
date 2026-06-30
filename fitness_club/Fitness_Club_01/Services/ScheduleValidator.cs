using Fitness_Club_01.Data;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Services;

public static class ScheduleValidator
{
    public const int ConcurrentVisitHours = 4;

    public static int DayNumberFromDate(DateOnly date) => date.DayOfWeek switch
    {
        System.DayOfWeek.Monday => 1,
        System.DayOfWeek.Tuesday => 2,
        System.DayOfWeek.Wednesday => 3,
        System.DayOfWeek.Thursday => 4,
        System.DayOfWeek.Friday => 5,
        System.DayOfWeek.Saturday => 6,
        System.DayOfWeek.Sunday => 7,
        _ => 0
    };

    public static bool TimeRangesOverlap(TimeOnly startA, int durationA, TimeOnly startB, int durationB)
    {
        var endA = startA.Add(TimeSpan.FromMinutes(durationA));
        var endB = startB.Add(TimeSpan.FromMinutes(durationB));
        return startA < endB && startB < endA;
    }

    public static async Task<string?> ValidateGroupClassScheduleAsync(
        ApplicationDbContext db,
        int idGymHall,
        int idDayOfWeek,
        TimeOnly startTime,
        int durationMinutes,
        int maxParticipants,
        bool isActive,
        int? excludeIdGroupClass = null)
    {
        var hall = await db.GymHalls.FindAsync(idGymHall);
        if (hall == null)
            return "Зал не найден";

        if (maxParticipants > hall.Capacity)
            return $"Максимум участников ({maxParticipants}) больше вместимости зала «{hall.HallName}» ({hall.Capacity})";

        if (!isActive)
            return null;

        var others = await db.GroupClasses
            .Where(g => g.IdGymHall == idGymHall
                        && g.IdDayOfWeek == idDayOfWeek
                        && g.IsActive
                        && (excludeIdGroupClass == null || g.IdGroupClass != excludeIdGroupClass))
            .ToListAsync();

        foreach (var other in others)
        {
            if (!TimeRangesOverlap(startTime, durationMinutes, other.StartTime, other.DurationMinutes))
                continue;

            return $"Время пересекается с занятием «{other.ClassName}» в этом зале ({other.StartTime:HH\\:mm}, {other.DurationMinutes} мин.)";
        }

        return null;
    }

    public static async Task<string?> ValidateBookingAsync(
        ApplicationDbContext db,
        GroupClass groupClass,
        DateOnly classDate,
        int? excludeIdClassBooking = null)
    {
        var dayError = BookingValidator.ValidateClassDate(groupClass, classDate);
        if (dayError != null)
            return dayError;

        var leadTimeError = BookingValidator.ValidateBookingLeadTime(groupClass, classDate);
        if (leadTimeError != null)
            return leadTimeError;

        if (await ClassSessionService.IsSessionCancelledAsync(db, groupClass.IdGroupClass, classDate))
            return $"Занятие «{groupClass.ClassName}» {classDate:dd.MM.yyyy} отменено тренером";

        var bookedOnClass = await db.ClassBookings.CountAsync(b =>
            b.IdGroupClass == groupClass.IdGroupClass
            && b.ClassDate == classDate
            && (excludeIdClassBooking == null || b.IdClassBooking != excludeIdClassBooking));

        if (bookedOnClass >= groupClass.MaxParticipants)
            return $"Нет свободных мест на занятие: {bookedOnClass}/{groupClass.MaxParticipants}";

        var hall = await db.GymHalls.FindAsync(groupClass.IdGymHall);
        if (hall == null)
            return "Зал не найден";

        var dayNumber = DayNumberFromDate(classDate);
        var classesInHall = await db.GroupClasses
            .Include(g => g.IdDayOfWeekNavigation)
            .Where(g => g.IdGymHall == groupClass.IdGymHall && g.IsActive)
            .ToListAsync();

        var overlapping = classesInHall
            .Where(g => g.IdDayOfWeekNavigation.DayNumber == dayNumber
                        && TimeRangesOverlap(
                            groupClass.StartTime,
                            groupClass.DurationMinutes,
                            g.StartTime,
                            g.DurationMinutes))
            .ToList();

        var totalInHall = 0;
        foreach (var cls in overlapping)
        {
            totalInHall += await db.ClassBookings.CountAsync(b =>
                b.IdGroupClass == cls.IdGroupClass
                && b.ClassDate == classDate
                && (excludeIdClassBooking == null || b.IdClassBooking != excludeIdClassBooking));
        }

        if (totalInHall >= hall.Capacity)
            return $"Зал «{hall.HallName}» переполнен в это время: {totalInHall}/{hall.Capacity}";

        return null;
    }

    public static async Task<string?> ValidateClubVisitCapacityAsync(ApplicationDbContext db)
    {
        var totalCapacity = await db.GymHalls.SumAsync(h => (int?)h.Capacity) ?? 0;
        if (totalCapacity <= 0)
            return null;

        var now = DateTimeDb.Now;
        var since = now.AddHours(-ConcurrentVisitHours);
        var inClub = await db.Visits.CountAsync(v => v.VisitDateTime >= since && v.VisitDateTime <= now);

        if (inClub >= totalCapacity)
            return $"Клуб заполнен: {inClub}/{totalCapacity} человек. Подождите около часа, пока освободятся места.";

        return null;
    }
}
