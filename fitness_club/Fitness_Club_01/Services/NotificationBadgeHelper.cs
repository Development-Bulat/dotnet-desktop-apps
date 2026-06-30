using Avalonia.Controls;
using Avalonia.Threading;
using Fitness_Club_01.Data;
using Fitness_Club_01.Services;

namespace Fitness_Club_01.Services;

public static class NotificationBadgeHelper
{
    public static void Attach(Window window, Button notificationsButton, Border badge, TextBlock badgeText)
    {
        async void Refresh()
        {
            if (Session.CurrentUser == null)
            {
                badge.IsVisible = false;
                return;
            }

            try
            {
                await using var db = new ApplicationDbContext();
                var count = await NotificationService.GetUnreadCountAsync(db, Session.CurrentUser.IdUserAccount);
                await Dispatcher.UIThread.InvokeAsync(() => UpdateBadge(badge, badgeText, count));
            }
            catch
            {
                await Dispatcher.UIThread.InvokeAsync(() => badge.IsVisible = false);
            }
        }

        window.Opened += (_, _) => Refresh();
        NotificationService.UnreadCountChanged += Refresh;

        window.Closed += (_, _) => NotificationService.UnreadCountChanged -= Refresh;
    }

    public static void UpdateBadge(Border badge, TextBlock badgeText, int count)
    {
        if (count <= 0)
        {
            badge.IsVisible = false;
            return;
        }

        badge.IsVisible = true;
        badgeText.Text = count > 9 ? "9+" : count.ToString();
    }
}
