using Avalonia.Controls;
using Fitness_Club_01.Controls;
using Fitness_Club_01.Services;

namespace Fitness_Club_01.Views;

public partial class TrainerWindow : Window
{
    public TrainerWindow()
    {
        InitializeComponent();
        ShellWindowHelper.ApplyUserCaption(UserCaptionText);
        ContentArea.Content = new TrainerClassSessionsControl();
        SessionsBtn.Click += (_, _) => ContentArea.Content = new TrainerClassSessionsControl();
        BookingsBtn.Click += (_, _) => ContentArea.Content = new BookingsControl(trainerOnly: true);
        ScheduleBtn.Click += (_, _) => ContentArea.Content = new GroupClassesControl(trainerOnly: true);
        NotificationsBtn.Click += (_, _) => ContentArea.Content = new NotificationsControl();
        ExitButton.Click += (_, _) => Logout();

        NotificationBadgeHelper.Attach(this, NotificationsBtn, NotificationBadge, NotificationBadgeText);
    }

    private void Logout()
    {
        Session.Logout();
        WindowNavigation.ReplaceMainWindow(this, new LoginWindow());
    }
}
