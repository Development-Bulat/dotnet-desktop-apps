namespace Fitness_Club_01.Services;

public static class DateValidator
{
    public static string? ValidateMembershipStart(DateOnly startDate)
    {
        var minDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-1));
        if (startDate < minDate)
            return "Дата начала не может быть раньше чем год назад";

        return null;
    }

    public static string? ValidateBookingDate(DateOnly classDate)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (classDate < today)
            return "Нельзя записаться на прошедшую дату";

        var maxDate = today.AddMonths(3);
        if (classDate > maxDate)
            return "Запись возможна не более чем на 3 месяца вперёд";

        return null;
    }
}
