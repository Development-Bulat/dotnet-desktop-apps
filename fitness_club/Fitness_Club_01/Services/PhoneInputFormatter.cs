using System.Text;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

namespace Fitness_Club_01.Services;

public static class PhoneInputFormatter
{
    public const int NationalDigitsLength = 10;
    public const string MaskPrefix = "+7 (";
    public const string EmptyMask = "+7 (___) ___-__-__";
    public const int DisplayMaxLength = 18;

    private sealed class State
    {
        public readonly StringBuilder Digits = new();
        public bool Updating;
    }

    public static void Attach(TextBox textBox)
    {
        var state = new State();
        textBox.Tag = state;

        textBox.MaxLength = DisplayMaxLength;
        textBox.Text = "";
        textBox.Watermark = EmptyMask;

        void ShowMask()
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (!textBox.IsFocused)
                    return;

                if (state.Digits.Length == 0)
                {
                    state.Updating = true;
                    try
                    {
                        textBox.Text = EmptyMask;
                        textBox.CaretIndex = MaskPrefix.Length;
                    }
                    finally
                    {
                        state.Updating = false;
                    }
                }
                else
                {
                    ApplyDisplay(textBox, state);
                }
            });
        }

        textBox.GotFocus += (_, _) => ShowMask();

        textBox.PointerPressed += (_, _) =>
        {
            if (state.Digits.Length == 0 && string.IsNullOrEmpty(textBox.Text))
                ShowMask();
        };

        textBox.LostFocus += (_, _) =>
        {
            if (state.Digits.Length == 0)
                textBox.Text = "";
        };

        textBox.KeyDown += (_, e) =>
        {
            if (!textBox.IsFocused)
                return;

            EnsureMaskOnFocus(textBox, state);

            if (e.Key == Key.Back)
            {
                if (state.Digits.Length > 0)
                {
                    state.Digits.Length--;
                    ApplyDisplay(textBox, state);
                }

                e.Handled = true;
                return;
            }

            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                return;
            }

            if (e.Key is Key.Left or Key.Right or Key.Home or Key.End or Key.Tab)
                return;

            if (TryGetDigit(e.Key, out var digit))
            {
                AppendDigit(textBox, state, digit);
                e.Handled = true;
                return;
            }

            if (!IsShortcut(e))
                e.Handled = true;
        };

        textBox.AddHandler(
            InputElement.TextInputEvent,
            (_, e) =>
            {
                if (!textBox.IsFocused || string.IsNullOrEmpty(e.Text))
                    return;

                EnsureMaskOnFocus(textBox, state);

                foreach (var ch in e.Text)
                {
                    if (char.IsDigit(ch) && state.Digits.Length < NationalDigitsLength)
                        state.Digits.Append(ch);
                }

                ApplyDisplay(textBox, state);
                e.Handled = true;
            },
            handledEventsToo: true);

        textBox.TextChanged += (_, _) =>
        {
            if (state.Updating)
                return;

            var extracted = ExtractNationalDigits(textBox.Text);
            if (extracted != state.Digits.ToString())
            {
                state.Digits.Clear();
                state.Digits.Append(extracted);
            }

            ApplyDisplay(textBox, state);
        };
    }

    public static void SetFromNormalized(TextBox textBox, string? normalizedPhone)
    {
        var state = GetState(textBox);
        state.Digits.Clear();

        if (!string.IsNullOrWhiteSpace(normalizedPhone))
            state.Digits.Append(ExtractNationalDigits(normalizedPhone));

        state.Updating = true;
        try
        {
            textBox.Text = state.Digits.Length == 0
                ? ""
                : FormatDisplay(state.Digits.ToString());
        }
        finally
        {
            state.Updating = false;
        }
    }

    public static string ExtractNationalDigits(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return "";

        var s = input;
        if (s.StartsWith(MaskPrefix, StringComparison.Ordinal))
            s = s[MaskPrefix.Length..];
        else if (s.StartsWith("+7", StringComparison.Ordinal))
            s = s[2..].TrimStart(' ', '(');

        var digits = new string(s.Where(char.IsDigit).ToArray());

        if (digits.Length > NationalDigitsLength)
            digits = digits[..NationalDigitsLength];

        return digits;
    }

    public static string FormatDisplay(string nationalDigits)
    {
        char Slot(int index) =>
            index < nationalDigits.Length ? nationalDigits[index] : '_';

        return $"+7 ({Slot(0)}{Slot(1)}{Slot(2)}) {Slot(3)}{Slot(4)}{Slot(5)}-{Slot(6)}{Slot(7)}-{Slot(8)}{Slot(9)}";
    }

    public static bool IsComplete(string? displayValue)
        => ExtractNationalDigits(displayValue).Length == NationalDigitsLength;

    public static string FormatStored(string? normalizedPhone)
    {
        if (string.IsNullOrWhiteSpace(normalizedPhone))
            return "—";

        var digits = ExtractNationalDigits(normalizedPhone);
        return digits.Length == NationalDigitsLength
            ? FormatDisplay(digits)
            : normalizedPhone;
    }

    private static State GetState(TextBox textBox)
    {
        if (textBox.Tag is State state)
            return state;

        state = new State();
        textBox.Tag = state;
        return state;
    }

    private static void EnsureMaskOnFocus(TextBox textBox, State state)
    {
        if (state.Digits.Length == 0 && string.IsNullOrEmpty(textBox.Text))
        {
            state.Updating = true;
            try
            {
                textBox.Text = EmptyMask;
                textBox.CaretIndex = MaskPrefix.Length;
            }
            finally
            {
                state.Updating = false;
            }
        }
    }

    private static void AppendDigit(TextBox textBox, State state, char digit)
    {
        if (state.Digits.Length >= NationalDigitsLength)
            return;

        state.Digits.Append(digit);
        ApplyDisplay(textBox, state);
    }

    private static void ApplyDisplay(TextBox textBox, State state)
    {
        var target = state.Digits.Length == 0
            ? textBox.IsFocused ? EmptyMask : ""
            : FormatDisplay(state.Digits.ToString());

        if (textBox.Text == target)
        {
            textBox.CaretIndex = CaretFor(state.Digits.Length);
            return;
        }

        state.Updating = true;
        try
        {
            textBox.Text = target;
            textBox.CaretIndex = CaretFor(state.Digits.Length);
        }
        finally
        {
            state.Updating = false;
        }
    }

    private static int CaretFor(int digitCount)
    {
        if (digitCount <= 0)
            return MaskPrefix.Length;

        var positions = new[] { 4, 5, 6, 9, 10, 11, 13, 14, 16, 17 };
        return digitCount >= NationalDigitsLength
            ? EmptyMask.Length
            : positions[Math.Min(digitCount, NationalDigitsLength - 1)];
    }

    private static bool TryGetDigit(Key key, out char digit)
    {
        if (key is >= Key.D0 and <= Key.D9)
        {
            digit = (char)('0' + (key - Key.D0));
            return true;
        }

        if (key is >= Key.NumPad0 and <= Key.NumPad9)
        {
            digit = (char)('0' + (key - Key.NumPad0));
            return true;
        }

        digit = default;
        return false;
    }

    private static bool IsShortcut(KeyEventArgs e) =>
        e.KeyModifiers is KeyModifiers.Control or KeyModifiers.Meta or KeyModifiers.Alt;
}
