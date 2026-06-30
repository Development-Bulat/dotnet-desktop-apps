using Avalonia.Controls;
using Avalonia.Interactivity;
using Fitness_Club_01.Data;
using Fitness_Club_01.Services;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Views;

public partial class StaffAccountWindow : Window
{
    private readonly ApplicationDbContext _db = new();
    private readonly int _idStaff;

    public bool Saved { get; private set; }

    public StaffAccountWindow(int idStaff)
    {
        _idStaff = idStaff;
        InitializeComponent();
        SaveButton.Click += OnSaveClick;
        CancelButton.Click += (_, _) => Close();
        Loaded += async (_, _) => await LoadAsync();
        Closed += (_, _) => _db.Dispose();
    }

    private async Task LoadAsync()
    {
        var staff = await _db.Staff.FindAsync(_idStaff);
        if (staff != null)
            StaffNameText.Text = PersonNameFormatter.FullName(staff);

        var roles = await _db.UserRoles
            .Where(r => r.RoleName == "Администратор" || r.RoleName == "Ресепшн")
            .OrderBy(r => r.RoleName)
            .ToListAsync();

        RoleCombo.ItemsSource = roles;
    }

    private async void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        var login = LoginBox.Text?.Trim();
        var password = PasswordBox.Text;

        if (RoleCombo.SelectedItem is not UserRole role)
        {
            ErrorText.Text = "Выберите роль";
            return;
        }

        var loginError = LoginValidator.Validate(login);
        if (loginError != null) { ErrorText.Text = loginError; return; }

        var passwordError = PasswordValidator.Validate(password);
        if (passwordError != null) { ErrorText.Text = passwordError; return; }

        if (await _db.UserAccounts.AnyAsync(u => u.Login == login))
        {
            ErrorText.Text = "Логин уже занят";
            return;
        }

        if (await _db.UserAccounts.AnyAsync(u => u.IdStaff == _idStaff))
        {
            ErrorText.Text = "У сотрудника уже есть учётная запись";
            return;
        }

        _db.UserAccounts.Add(new UserAccount
        {
            Login = login!,
            PasswordHash = password!,
            IdUserRole = role.IdUserRole,
            IdStaff = _idStaff
        });

        await _db.SaveChangesAsync();
        Saved = true;
        Close();
    }
}
