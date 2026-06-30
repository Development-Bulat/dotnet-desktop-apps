namespace Fitness_Club_01.Services;

/// <summary>Даты для PostgreSQL timestamp without time zone.</summary>
public static class DateTimeDb
{
    public static DateTime Now =>
        DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

    public static string FormatLocal(DateTime value) =>
        value.ToString("dd.MM.yyyy HH:mm");
}
