namespace Fitness_Club_01.Services;

public static class PhoneValidator
{
    public static string? ValidateAndNormalize(string? phone, out string normalized)
    {
        normalized = "";

        if (string.IsNullOrWhiteSpace(phone))
            return "Введите номер телефона";

        var digits = PhoneInputFormatter.ExtractNationalDigits(phone);

        if (digits.Length != PhoneInputFormatter.NationalDigitsLength)
            return "Введите номер полностью: +7 (900) 123-45-67";

        if (digits[0] != '9')
            return "Мобильный номер должен начинаться с 9";

        normalized = "+7" + digits;
        return null;
    }

    public static string? ValidateOptionalAndNormalize(string? phone, out string? normalized)
    {
        normalized = null;

        if (string.IsNullOrWhiteSpace(phone) ||
            phone == PhoneInputFormatter.EmptyMask ||
            PhoneInputFormatter.ExtractNationalDigits(phone).Length == 0)
            return null;

        var error = ValidateAndNormalize(phone, out var required);
        if (error != null)
            return error;

        normalized = required;
        return null;
    }
}
