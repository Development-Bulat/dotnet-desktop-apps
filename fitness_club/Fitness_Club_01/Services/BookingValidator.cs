using Fitness_Club_01.Data;

namespace Fitness_Club_01.Services;

public static class BookingValidator
{
    public static string? ValidateClassDate(GroupClass groupClass, DateOnly classDate)
    {
        var dayNumber = ScheduleValidator.DayNumberFromDate(classDate);

        if (groupClass.IdDayOfWeekNavigation.DayNumber != dayNumber)
            return $"Занятие «{groupClass.ClassName}» проводится по {groupClass.IdDayOfWeekNavigation.DayName}";

        return null;
    }

    public static string? ValidateBookingLeadTime(GroupClass groupClass, DateOnly classDate)
        => ValidateLeadTime(groupClass.StartTime, classDate, "Записаться можно не позднее чем за 24 часа до начала занятия");

    public static string? ValidateCancelLeadTime(TimeOnly startTime, DateOnly classDate)
        => ValidateLeadTime(startTime, classDate, "Отменить запись можно не позднее чем за 24 часа до начала занятия");

    private static string? ValidateLeadTime(TimeOnly startTime, DateOnly classDate, string message)
    {
        var classStart = classDate.ToDateTime(startTime);
        var deadline = DateTimeDb.Now.AddHours(24);

        if (classStart < deadline)
            return message;

        return null;
    }
}
