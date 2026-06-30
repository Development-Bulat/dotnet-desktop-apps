using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Fitness_Club_01.Services;

public enum MessageBoxButtons
{
    Ok,
    YesNo
}

public enum MessageBoxResult
{
    Ok,
    Yes,
    No
}

public static class MessageBox
{
    public static async Task<MessageBoxResult> Show(Window parent, string text, string title, MessageBoxButtons buttons)
    {
        var result = MessageBoxResult.Ok;

        var dialog = new Window
        {
            Title = title,
            Width = 380,
            Height = 170,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Background = new SolidColorBrush(Color.Parse("#F8FAFC"))
        };

        var mainPanel = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 20,
            VerticalAlignment = VerticalAlignment.Center
        };

        mainPanel.Children.Add(new TextBlock
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.Parse("#0F172A")),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center
        });

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        if (buttons == MessageBoxButtons.Ok)
        {
            var okButton = CreateButton("OK", "#10B981");
            okButton.Click += (_, _) => dialog.Close();
            buttonPanel.Children.Add(okButton);
        }
        else
        {
            var yesButton = CreateButton("Да", "#EF4444");
            yesButton.Click += (_, _) =>
            {
                result = MessageBoxResult.Yes;
                dialog.Close();
            };

            var noButton = CreateButton("Нет", "#64748B");
            noButton.Click += (_, _) =>
            {
                result = MessageBoxResult.No;
                dialog.Close();
            };

            buttonPanel.Children.Add(yesButton);
            buttonPanel.Children.Add(noButton);
        }

        mainPanel.Children.Add(buttonPanel);
        dialog.Content = mainPanel;

        await dialog.ShowDialog(parent);
        return result;
    }

    private static Button CreateButton(string text, string color) => new()
    {
        Content = text,
        Width = 90,
        Height = 36,
        Background = new SolidColorBrush(Color.Parse(color)),
        Foreground = Brushes.White,
        FontWeight = FontWeight.SemiBold,
        HorizontalContentAlignment = HorizontalAlignment.Center,
        VerticalContentAlignment = VerticalAlignment.Center
    };
}
