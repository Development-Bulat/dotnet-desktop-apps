using Avalonia.Controls;
using Fitness_Club_01.Controls;
using Fitness_Club_01.Services;
using Fitness_Club_01.Views;

namespace Fitness_Club_01.Views;

public partial class AdminWindow : Window
{
    public AdminWindow()
    {
        InitializeComponent();
        ShellWindowHelper.ApplyUserCaption(UserCaptionText);
        ContentArea.Content = new DashboardControl();

        DashboardBtn.Click += (_, _) => ContentArea.Content = new DashboardControl();
        ClientsBtn.Click += (_, _) => ContentArea.Content = new ClientsControl();
        MembershipsBtn.Click += (_, _) => ContentArea.Content = new MembershipsControl();
        VisitsBtn.Click += (_, _) => ContentArea.Content = new VisitsControl();
        BookingsBtn.Click += (_, _) => ContentArea.Content = new BookingsControl();
        ScheduleBtn.Click += (_, _) => ContentArea.Content = new GroupClassesControl();
        StaffBtn.Click += (_, _) => ContentArea.Content = new StaffControl();
        TrainersBtn.Click += (_, _) => ContentArea.Content = new TrainersControl();
        ReferencesBtn.Click += (_, _) => ContentArea.Content = new ReferencesControl();
        ReportsBtn.Click += (_, _) => ContentArea.Content = new ReportsControl();
        ExitButton.Click += (_, _) => Logout();
    }

    private void Logout()
    {
        Session.Logout();
        WindowNavigation.ReplaceMainWindow(this, new LoginWindow());
    }
}
