namespace Fitness_Club_01.Services;

public static class NameValidator
{
    public static string? ValidateRequired(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return $"Укажите {fieldName.ToLower()}";

        return ValidateOptional(value, fieldName, required: true);
    }

    public static string? ValidateOptional(string? value, string fieldName, bool required = false)
    {
        if (string.IsNullOrWhiteSpace(value))
            return required ? $"Укажите {fieldName.ToLower()}" : null;

        var trimmed = value.Trim();

        if (trimmed.Length < InputLimits.NameMin)
            return $"{fieldName}: минимум {InputLimits.NameMin} символа";

        if (trimmed.Length > InputLimits.NameMax)
            return $"{fieldName}: максимум {InputLimits.NameMax} символов";

        if (!trimmed.All(c => char.IsLetter(c) || c == '-' || c == ' '))
            return $"{fieldName}: только буквы, пробел и дефис";

        return null;
    }
}
