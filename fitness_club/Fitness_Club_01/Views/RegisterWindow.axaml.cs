using Avalonia.Controls;
using Avalonia.Interactivity;
using Fitness_Club_01.Data;
using Fitness_Club_01.Services;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Views;

public partial class RegisterWindow : Window
{
    private readonly ApplicationDbContext _db = new();
    private readonly LoginWindow? _loginWindow;
    private bool _enteredApp;

    public RegisterWindow(LoginWindow? loginWindow = null)
    {
        _loginWindow = loginWindow;
        InitializeComponent();
        PhoneInputFormatter.Attach(PhoneBox);
        RegisterButton.Click += OnRegisterClick;
        LoginLink.Click += OnLoginClick;
        Closed += OnClosed;

        void ClearError(object? _, EventArgs __) => ErrorText.Text = "";
        LastNameBox.TextChanged += ClearError;
        FirstNameBox.TextChanged += ClearError;
        PatronymicBox.TextChanged += ClearError;
        PhoneBox.TextChanged += ClearError;
        LoginBox.TextChanged += ClearError;
        PasswordBox.TextChanged += ClearError;
        ConfirmPasswordBox.TextChanged += ClearError;
        BirthDatePicker.SelectedDateChanged += (_, _) => ErrorText.Text = "";
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _db.Dispose();

        if (!_enteredApp && _loginWindow != null && !_loginWindow.IsVisible)
            WindowNavigation.ShowExistingMain(_loginWindow);
    }

    private async void OnRegisterClick(object? sender, RoutedEventArgs e)
    {
        var lastName = LastNameBox.Text?.Trim();
        var firstName = FirstNameBox.Text?.Trim();
        var patronymic = PatronymicBox.Text?.Trim();
        var phoneInput = PhoneBox.Text?.Trim();
        var login = LoginBox.Text?.Trim();
        var password = PasswordBox.Text;
        var confirm = ConfirmPasswordBox.Text;

        var lastNameError = NameValidator.ValidateRequired(lastName, "Фамилию");
        if (lastNameError != null) { ErrorText.Text = lastNameError; return; }

        var firstNameError = NameValidator.ValidateRequired(firstName, "Имя");
        if (firstNameError != null) { ErrorText.Text = firstNameError; return; }

        var patronymicError = NameValidator.ValidateOptional(patronymic, "Отчество");
        if (patronymicError != null) { ErrorText.Text = patronymicError; return; }

        var phoneError = PhoneValidator.ValidateAndNormalize(phoneInput, out var phone);
        if (phoneError != null) { ErrorText.Text = phoneError; return; }

        DateOnly? birthDate = BirthDatePicker.SelectedDate.HasValue
            ? DateOnly.FromDateTime(BirthDatePicker.SelectedDate.Value.DateTime)
            : null;

        var birthError = BirthDateValidator.Validate(birthDate);
        if (birthError != null) { ErrorText.Text = birthError; return; }

        var loginError = LoginValidator.Validate(login);
        if (loginError != null) { ErrorText.Text = loginError; return; }

        var passwordError = PasswordValidator.Validate(password);
        if (passwordError != null) { ErrorText.Text = passwordError; return; }

        if (password != confirm)
        {
            ErrorText.Text = "Пароли не совпадают";
            return;
        }

        if (await _db.UserAccounts.AnyAsync(u => u.Login == login))
        {
            ErrorText.Text = "Пользователь с таким логином уже существует";
            return;
        }

        if (await _db.Clients.AnyAsync(c => c.Phone == phone))
        {
            ErrorText.Text = "Клиент с таким телефоном уже существует";
            return;
        }

        var clientRole = await _db.UserRoles.FirstOrDefaultAsync(r => r.RoleName == "Клиент");
        if (clientRole == null)
        {
            ErrorText.Text = "Роль «Клиент» не найдена в БД";
            return;
        }

        var client = new Client
        {
            LastName = lastName!,
            FirstName = firstName!,
            Patronymic = string.IsNullOrWhiteSpace(patronymic) ? null : patronymic,
            Phone = phone,
            BirthDate = birthDate,
            RegisteredAt = DateTimeDb.Now
        };

        _db.Clients.Add(client);
        await _db.SaveChangesAsync();

        _db.UserAccounts.Add(new UserAccount
        {
            Login = login!,
            PasswordHash = password!,
            IdUserRole = clientRole.IdUserRole,
            IdClient = client.IdClient
        });

        await _db.SaveChangesAsync();

        Session.CurrentUser = await _db.UserAccounts
            .Include(u => u.IdUserRoleNavigation)
            .Include(u => u.IdClientNavigation)
            .FirstAsync(u => u.Login == login);
        Session.CurrentRole = Session.CurrentUser.IdUserRoleNavigation;

        _enteredApp = true;
        WindowNavigation.EnterApplication(this, _loginWindow, new ClientWindow());
    }

    private void OnLoginClick(object? sender, RoutedEventArgs e)
    {
        if (_loginWindow != null)
        {
            WindowNavigation.ShowExistingMain(_loginWindow);
            Close();
            return;
        }

        WindowNavigation.ReplaceMainWindow(this, new LoginWindow());
    }
}
