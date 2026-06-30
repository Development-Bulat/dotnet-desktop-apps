namespace Fitness_Club_01.Services;

public static class PasswordValidator
{
    public static string? Validate(string? password)
    {
        if (string.IsNullOrEmpty(password))
            return "Введите пароль";

        if (password.Length < InputLimits.PasswordMin || password.Length > InputLimits.PasswordMax)
            return $"Пароль должен содержать от {InputLimits.PasswordMin} до {InputLimits.PasswordMax} символов";

        if (!password.Any(char.IsUpper))
            return "Пароль должен содержать хотя бы одну заглавную букву";

        if (!password.Any(char.IsLower))
            return "Пароль должен содержать хотя бы одну строчную букву";

        if (!password.Any(char.IsDigit))
            return "Пароль должен содержать хотя бы одну цифру";

        if (!password.Any(c => !char.IsLetterOrDigit(c)))
            return "Пароль должен содержать хотя бы один спецсимвол (!@#$% и т.д.)";

        return null;
    }
}
