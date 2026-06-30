using Avalonia.Controls;
using Fitness_Club_01.Controls;
using Fitness_Club_01.Services;

namespace Fitness_Club_01.Views;

public partial class ReceptionWindow : Window
{
    public ReceptionWindow()
    {
        InitializeComponent();
        ShellWindowHelper.ApplyUserCaption(UserCaptionText);
        ContentArea.Content = new ClientsControl();

        ClientsBtn.Click += (_, _) => ContentArea.Content = new ClientsControl();
        MembershipsBtn.Click += (_, _) => ContentArea.Content = new MembershipsControl();
        VisitsBtn.Click += (_, _) => ContentArea.Content = new VisitsControl();
        BookingsBtn.Click += (_, _) => ContentArea.Content = new BookingsControl();
        ExitButton.Click += (_, _) => Logout();
    }

    private void Logout()
    {
        Session.Logout();
        WindowNavigation.ReplaceMainWindow(this, new LoginWindow());
    }
}
