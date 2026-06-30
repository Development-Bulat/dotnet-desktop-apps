using Avalonia.Controls;
using Avalonia.Interactivity;
using Fitness_Club_01.Data;
using Fitness_Club_01.Services;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Views;

public partial class ChangePasswordWindow : Window
{
    private readonly ApplicationDbContext _db = new();

    public bool Saved { get; private set; }

    public ChangePasswordWindow()
    {
        InitializeComponent();
        SaveButton.Click += OnSaveClick;
        CancelButton.Click += (_, _) => Close();
        Closed += (_, _) => _db.Dispose();
    }

    private async void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        if (Session.CurrentUser == null)
        {
            ErrorText.Text = "Сессия недействительна";
            return;
        }

        var current = CurrentPasswordBox.Text ?? "";
        var newPassword = NewPasswordBox.Text ?? "";
        var confirm = ConfirmPasswordBox.Text ?? "";

        var user = await _db.UserAccounts.FindAsync(Session.CurrentUser.IdUserAccount);
        if (user == null)
        {
            ErrorText.Text = "Пользователь не найден";
            return;
        }

        if (user.PasswordHash != current)
        {
            ErrorText.Text = "Неверный текущий пароль";
            return;
        }

        var passwordError = PasswordValidator.Validate(newPassword);
        if (passwordError != null)
        {
            ErrorText.Text = passwordError;
            return;
        }

        if (newPassword != confirm)
        {
            ErrorText.Text = "Пароли не совпадают";
            return;
        }

        user.PasswordHash = newPassword;
        await _db.SaveChangesAsync();
        Session.CurrentUser.PasswordHash = user.PasswordHash;
        Saved = true;
        Close();
    }
}
