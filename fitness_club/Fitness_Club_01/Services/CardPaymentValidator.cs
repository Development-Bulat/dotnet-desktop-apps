using System.Globalization;
using System.Text.RegularExpressions;

namespace Fitness_Club_01.Services;

public static class CardPaymentValidator
{
    private static readonly Regex HolderRegex = new(@"^[A-Za-zА-Яа-яЁё\s\-']{2,50}$", RegexOptions.Compiled);

    public static string? ValidateCardNumber(string digits)
    {
        if (digits.Length != 16)
            return "Введите 16 цифр номера карты";

        if (!digits.All(char.IsDigit))
            return "Номер карты должен содержать только цифры";

        return null;
    }

    public static string? ValidateExpiry(string expiry)
    {
        if (string.IsNullOrWhiteSpace(expiry))
            return "Укажите срок действия карты (ММ/ГГ)";

        var digits = new string(expiry.Where(char.IsDigit).ToArray());
        if (digits.Length != 4)
            return "Срок действия: 4 цифры в формате ММ/ГГ (например, 11/30)";

        if (!int.TryParse(digits[..2], NumberStyles.None, CultureInfo.InvariantCulture, out var month)
            || month is < 1 or > 12)
        {
            return "Месяц: от 01 до 12. Сначала месяц, затем год (ММ/ГГ)";
        }

        if (!int.TryParse(digits[2..], NumberStyles.None, CultureInfo.InvariantCulture, out var yearShort))
            return "Укажите год карты (две последние цифры)";

        var year = 2000 + yearShort;
        var today = DateOnly.FromDateTime(DateTime.Today);
        var currentYear = today.Year;
        var currentMonth = today.Month;

        if (year < currentYear)
            return $"Срок действия карты истёк: год {year} уже прошёл";

        if (year == currentYear && month < currentMonth)
        {
            return $"Срок действия карты истёк: {month:D2}/{yearShort:D2} — этот месяц уже прошёл";
        }

        var expiryDate = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
        if (expiryDate < today)
            return "Срок действия карты истёк";

        return null;
    }

    public static string? ValidateCvv(string cvv)
    {
        if (cvv.Length != 3 || !cvv.All(char.IsDigit))
            return "CVV: 3 цифры";

        return null;
    }

    public static string? ValidateHolder(string holder)
    {
        var trimmed = holder.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return "Укажите имя держателя карты";

        if (!HolderRegex.IsMatch(trimmed))
            return "Имя держателя: буквы, 2–50 символов";

        return null;
    }
}
