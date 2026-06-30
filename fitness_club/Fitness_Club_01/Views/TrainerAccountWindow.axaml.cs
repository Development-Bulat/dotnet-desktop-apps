using Avalonia.Controls;
using Avalonia.Interactivity;
using Fitness_Club_01.Data;
using Fitness_Club_01.Services;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Views;

public partial class TrainerAccountWindow : Window
{
    private readonly ApplicationDbContext _db = new();
    private readonly int _idTrainer;

    public bool Saved { get; private set; }

    public TrainerAccountWindow(int idTrainer)
    {
        _idTrainer = idTrainer;
        InitializeComponent();
        SaveButton.Click += OnSaveClick;
        CancelButton.Click += (_, _) => Close();
        Loaded += async (_, _) => await LoadAsync();
        Closed += (_, _) => _db.Dispose();
    }

    private async Task LoadAsync()
    {
        var trainer = await _db.Trainers.FindAsync(_idTrainer);
        if (trainer != null)
            TrainerNameText.Text = PersonNameFormatter.FullName(trainer);
    }

    private async void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        var login = LoginBox.Text?.Trim();
        var password = PasswordBox.Text;

        var loginError = LoginValidator.Validate(login);
        if (loginError != null) { ErrorText.Text = loginError; return; }

        var passwordError = PasswordValidator.Validate(password);
        if (passwordError != null) { ErrorText.Text = passwordError; return; }

        if (await _db.UserAccounts.AnyAsync(u => u.Login == login))
        {
            ErrorText.Text = "Логин уже занят";
            return;
        }

        if (await _db.UserAccounts.AnyAsync(u => u.IdTrainer == _idTrainer))
        {
            ErrorText.Text = "У тренера уже есть учётная запись";
            return;
        }

        var role = await _db.UserRoles.FirstAsync(r => r.RoleName == "Тренер");

        _db.UserAccounts.Add(new UserAccount
        {
            Login = login!,
            PasswordHash = password!,
            IdUserRole = role.IdUserRole,
            IdTrainer = _idTrainer
        });

        await _db.SaveChangesAsync();
        Saved = true;
        Close();
    }
}
