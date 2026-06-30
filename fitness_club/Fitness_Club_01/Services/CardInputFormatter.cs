using System.Globalization;
using System.Text;
using Avalonia.Controls;
using Avalonia.Input;

namespace Fitness_Club_01.Services;

public static class CardInputFormatter
{
    private sealed class DigitState
    {
        public readonly StringBuilder Digits = new();
        public bool Updating;
    }

    public static void AttachCardNumber(TextBox textBox)
    {
        var state = new DigitState();
        textBox.Tag = state;
        textBox.MaxLength = 19;
        textBox.Watermark = "0000 0000 0000 0000";

        textBox.TextInput += (_, e) =>
        {
            if (e.Text?.Length == 1 && !char.IsDigit(e.Text[0]))
                e.Handled = true;
        };

        textBox.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Back && state.Digits.Length > 0)
            {
                state.Digits.Length--;
                ApplyCardDisplay(textBox, state);
                e.Handled = true;
            }
        };

        textBox.TextChanged += (_, _) =>
        {
            if (state.Updating)
                return;

            var digits = new string((textBox.Text ?? "").Where(char.IsDigit).ToArray());
            if (digits.Length > 16)
                digits = digits[..16];

            state.Digits.Clear();
            state.Digits.Append(digits);
            ApplyCardDisplay(textBox, state);
        };
    }

    public static string GetCardDigits(TextBox textBox) =>
        textBox.Tag is DigitState state ? state.Digits.ToString() : new string((textBox.Text ?? "").Where(char.IsDigit).ToArray());

    public static void AttachExpiry(TextBox textBox)
    {
        var state = new DigitState();
        textBox.Tag = state;
        textBox.MaxLength = 5;
        textBox.Watermark = "ММ/ГГ";

        textBox.TextInput += (_, e) =>
        {
            if (e.Text?.Length == 1 && !char.IsDigit(e.Text[0]))
                e.Handled = true;
        };

        textBox.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Back && state.Digits.Length > 0)
            {
                state.Digits.Length--;
                ApplyExpiryDisplay(textBox, state);
                e.Handled = true;
            }
        };

        textBox.TextChanged += (_, _) =>
        {
            if (state.Updating)
                return;

            var digits = new string((textBox.Text ?? "").Where(char.IsDigit).ToArray());
            state.Digits.Clear();
            state.Digits.Append(NormalizeExpiryDigits(digits));
            ApplyExpiryDisplay(textBox, state);
        };
    }

    public static string GetExpiryDigits(TextBox textBox) =>
        textBox.Tag is DigitState state ? state.Digits.ToString() : new string((textBox.Text ?? "").Where(char.IsDigit).ToArray());

    public static void AttachCvv(TextBox textBox)
    {
        textBox.MaxLength = 3;
        textBox.Watermark = "CVV";
        textBox.PasswordChar = '•';

        textBox.TextInput += (_, e) =>
        {
            if (e.Text?.Length == 1 && !char.IsDigit(e.Text[0]))
                e.Handled = true;
        };

        textBox.TextChanged += (_, _) =>
        {
            var digits = new string((textBox.Text ?? "").Where(char.IsDigit).ToArray());
            if (digits.Length > 3)
                digits = digits[..3];

            if (textBox.Text != digits)
                textBox.Text = digits;
        };
    }

    private static string NormalizeExpiryDigits(string digits)
    {
        if (digits.Length == 0)
            return "";

        if (digits.Length == 1 && digits[0] >= '2' && digits[0] <= '9')
            return "0" + digits[0];

        if (digits.Length >= 2)
        {
            var month = int.Parse(digits[..2], CultureInfo.InvariantCulture);
            if (month == 0)
                digits = "01" + digits[2..];
            else if (month > 12)
                digits = "12" + digits[2..];
        }

        return digits.Length > 4 ? digits[..4] : digits;
    }

    private static void ApplyExpiryDisplay(TextBox textBox, DigitState state)
    {
        var digits = state.Digits.ToString();
        var formatted = digits.Length switch
        {
            0 => "",
            <= 2 => digits,
            _ => $"{digits[..2]}/{digits[2..]}"
        };

        state.Updating = true;
        try
        {
            textBox.Text = formatted;
            textBox.CaretIndex = formatted.Length;
        }
        finally
        {
            state.Updating = false;
        }
    }

    private static void ApplyCardDisplay(TextBox textBox, DigitState state)
    {
        var digits = state.Digits.ToString();
        var parts = new List<string>();
        for (var i = 0; i < digits.Length; i += 4)
            parts.Add(digits.Substring(i, Math.Min(4, digits.Length - i)));

        var formatted = string.Join(' ', parts);

        state.Updating = true;
        try
        {
            textBox.Text = formatted;
            textBox.CaretIndex = formatted.Length;
        }
        finally
        {
            state.Updating = false;
        }
    }
}
