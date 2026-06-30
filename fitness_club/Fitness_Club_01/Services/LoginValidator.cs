namespace Fitness_Club_01.Services;

public static class LoginValidator
{
    public static string? Validate(string? login)
    {
        if (string.IsNullOrWhiteSpace(login))
            return "Введите логин";

        var trimmed = login.Trim();

        if (trimmed.Length < InputLimits.LoginMin)
            return $"Логин: минимум {InputLimits.LoginMin} символа";

        if (trimmed.Length > InputLimits.LoginMax)
            return $"Логин: максимум {InputLimits.LoginMax} символов";

        if (!trimmed.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '.'))
            return "Логин: только буквы, цифры, _ и .";

        return null;
    }
}
