namespace Fitness_Club_01.Services;

public static class BirthDateValidator
{
    public const int MinAgeYears = 18;

    public static DateTime MaxBirthDate => DateTime.Today.AddYears(-MinAgeYears);

    public static string? Validate(DateOnly? birthDate)
    {
        if (!birthDate.HasValue)
            return "Укажите дату рождения";

        var maxDate = DateOnly.FromDateTime(MaxBirthDate);
        if (birthDate.Value > maxDate)
            return $"Регистрация доступна только совершеннолетним (от {MinAgeYears} лет)";

        return null;
    }
}
