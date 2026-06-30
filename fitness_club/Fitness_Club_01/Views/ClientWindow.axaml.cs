using Avalonia.Controls;
using Fitness_Club_01.Controls;
using Fitness_Club_01.Services;
using Fitness_Club_01.Views;

namespace Fitness_Club_01.Views;

public partial class ClientWindow : Window
{
    public ClientWindow()
    {
        InitializeComponent();
        ShellWindowHelper.ApplyUserCaption(UserCaptionText);
        ContentArea.Content = new MembershipsControl(clientOnly: true);

        MembershipBtn.Click += (_, _) => ContentArea.Content = new MembershipsControl(clientOnly: true);
        BookingsBtn.Click += (_, _) => ContentArea.Content = new BookingsControl(clientOnly: true);
        BookClassBtn.Click += async (_, _) => await OnBookClassAsync();
        NotificationsBtn.Click += (_, _) => ContentArea.Content = new NotificationsControl();
        ProfileBtn.Click += async (_, _) => await OnProfileAsync();
        PasswordBtn.Click += async (_, _) => await OnPasswordAsync();
        ExitButton.Click += (_, _) => Logout();

        NotificationBadgeHelper.Attach(this, NotificationsBtn, NotificationBadge, NotificationBadgeText);
    }

    private async Task OnBookClassAsync()
    {
        if (Session.CurrentUser?.IdClient == null) return;

        var win = new BookingEditWindow(Session.CurrentUser.IdClient);
        await win.ShowDialog(this);
        if (win.Saved)
            ContentArea.Content = new BookingsControl(clientOnly: true);
    }

    private async Task OnProfileAsync()
    {
        if (Session.CurrentUser?.IdClient == null) return;

        var win = new ClientEditWindow(Session.CurrentUser.IdClient);
        await win.ShowDialog(this);
    }

    private async Task OnPasswordAsync()
    {
        var win = new ChangePasswordWindow();
        await win.ShowDialog(this);
    }

    private void Logout()
    {
        Session.Logout();
        WindowNavigation.ReplaceMainWindow(this, new LoginWindow());
    }
}
