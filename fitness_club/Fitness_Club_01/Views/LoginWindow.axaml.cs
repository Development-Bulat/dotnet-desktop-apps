using Avalonia.Controls;
using Avalonia.Interactivity;
using Fitness_Club_01.Data;
using Fitness_Club_01.Services;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Views;

public partial class LoginWindow : Window
{
    private readonly ApplicationDbContext _db = new();

    public LoginWindow()
    {
        InitializeComponent();
        LoginButton.Click += OnLoginClick;
        RegisterLink.Click += OnRegisterClick;
        Closed += (_, _) => _db.Dispose();
    }

    private async void OnLoginClick(object? sender, RoutedEventArgs e)
    {
        var login = LoginBox.Text?.Trim();
        var password = PasswordBox.Text;

        var loginError = LoginValidator.Validate(login);
        if (loginError != null)
        {
            ErrorText.Text = loginError;
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ErrorText.Text = "Введите пароль";
            return;
        }

        var user = await _db.UserAccounts
            .Include(u => u.IdUserRoleNavigation)
            .Include(u => u.IdClientNavigation)
            .Include(u => u.IdStaffNavigation)
            .Include(u => u.IdTrainerNavigation)
            .FirstOrDefaultAsync(u => u.Login == login && u.PasswordHash == password);

        if (user == null)
        {
            ErrorText.Text = "Неверный логин или пароль";
            return;
        }

        Session.CurrentUser = user;
        Session.CurrentRole = user.IdUserRoleNavigation;

        OpenRoleWindow(user.IdUserRoleNavigation.RoleName);
    }

    private void OpenRoleWindow(string roleName)
    {
        Window window = roleName switch
        {
            "Администратор" => new AdminWindow(),
            "Ресепшн" => new ReceptionWindow(),
            "Тренер" => new TrainerWindow(),
            "Клиент" => new ClientWindow(),
            _ => new LoginWindow()
        };
        WindowNavigation.ReplaceMainWindow(this, window);
    }

    private void OnRegisterClick(object? sender, RoutedEventArgs e)
    {
        WindowNavigation.OpenCompanionWindow(this, new RegisterWindow(this));
    }
}
