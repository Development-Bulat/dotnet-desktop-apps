using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Fitness_Club_01.Services;

public static class ShellContentFactory
{
    public static StackPanel CreateWelcome(string roleTitle, string? subtitle = null)
    {
        var firstName = Session.CurrentUser?.IdClientNavigation != null
            ? Session.CurrentUser.IdClientNavigation.FirstName
            : Session.CurrentUser?.IdStaffNavigation != null
                ? Session.CurrentUser.IdStaffNavigation.FirstName
                : Session.CurrentUser?.IdTrainerNavigation != null
                    ? Session.CurrentUser.IdTrainerNavigation.FirstName
                    : "Пользователь";

        return new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 12,
            Children =
            {
                new Border
                {
                    Width = 72,
                    Height = 72,
                    CornerRadius = new CornerRadius(20),
                    Background = new SolidColorBrush(Color.Parse("#10B981")),
                    Child = new TextBlock
                    {
                        Text = "FC",
                        FontSize = 28,
                        FontWeight = FontWeight.Bold,
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                },
                new TextBlock
                {
                    Text = $"Здравствуйте, {firstName}!",
                    FontSize = 24,
                    FontWeight = FontWeight.Bold,
                    Foreground = new SolidColorBrush(Color.Parse("#0F172A")),
                    HorizontalAlignment = HorizontalAlignment.Center
                },
                new TextBlock
                {
                    Text = roleTitle,
                    FontSize = 15,
                    Foreground = new SolidColorBrush(Color.Parse("#64748B")),
                    HorizontalAlignment = HorizontalAlignment.Center
                },
                new TextBlock
                {
                    Text = subtitle ?? "Выберите раздел в меню слева",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Color.Parse("#94A3B8")),
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            }
        };
    }
}
