using Avalonia.Controls;
using Avalonia.Interactivity;
using Fitness_Club_01.Data;
using Fitness_Club_01.Services;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Views;

public partial class ClientAccountWindow : Window
{
    private readonly ApplicationDbContext _db = new();
    private readonly int _idClient;

    public bool Saved { get; private set; }

    public ClientAccountWindow(int idClient)
    {
        _idClient = idClient;
        InitializeComponent();
        SaveButton.Click += OnSaveClick;
        CancelButton.Click += (_, _) => Close();
        Loaded += async (_, _) => await LoadAsync();
        Closed += (_, _) => _db.Dispose();
    }

    private async Task LoadAsync()
    {
        var client = await _db.Clients.FindAsync(_idClient);
        if (client != null)
            ClientNameText.Text = PersonNameFormatter.FullName(client);
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

        if (await _db.UserAccounts.AnyAsync(u => u.IdClient == _idClient))
        {
            ErrorText.Text = "У клиента уже есть учётная запись";
            return;
        }

        var role = await _db.UserRoles.FirstAsync(r => r.RoleName == "Клиент");
        _db.UserAccounts.Add(new UserAccount
        {
            Login = login!,
            PasswordHash = password!,
            IdUserRole = role.IdUserRole,
            IdClient = _idClient
        });

        await _db.SaveChangesAsync();
        Saved = true;
        Close();
    }
}
