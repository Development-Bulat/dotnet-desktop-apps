using Avalonia.Controls;
using Avalonia.Interactivity;
using Fitness_Club_01.Data;
using Fitness_Club_01.Services;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Views;

public partial class ClientEditWindow : Window
{
    private readonly ApplicationDbContext _db = new();
    private readonly int? _idClient;

    public bool Saved { get; private set; }

    public ClientEditWindow(int? idClient = null)
    {
        _idClient = idClient;
        InitializeComponent();
        PhoneInputFormatter.Attach(PhoneBox);
        SaveButton.Click += OnSaveClick;
        CancelButton.Click += (_, _) => Close();
        Loaded += async (_, _) => await LoadAsync();
        Closed += (_, _) => _db.Dispose();
    }

    private async Task LoadAsync()
    {
        if (_idClient == null) return;

        TitleText.Text = "Редактирование клиента";
        var client = await _db.Clients.FindAsync(_idClient);
        if (client == null) return;

        LastNameBox.Text = client.LastName;
        FirstNameBox.Text = client.FirstName;
        PatronymicBox.Text = client.Patronymic;
        PhoneInputFormatter.SetFromNormalized(PhoneBox, client.Phone);
        if (client.BirthDate.HasValue)
            BirthDatePicker.SelectedDate = client.BirthDate.Value.ToDateTime(TimeOnly.MinValue);
    }

    private async void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        var lastName = LastNameBox.Text?.Trim();
        var firstName = FirstNameBox.Text?.Trim();
        var patronymic = PatronymicBox.Text?.Trim();
        var phoneInput = PhoneBox.Text?.Trim();

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

        if (await _db.Clients.AnyAsync(c => c.Phone == phone && c.IdClient != (_idClient ?? 0)))
        {
            ErrorText.Text = "Телефон уже используется";
            return;
        }

        Client client;
        if (_idClient.HasValue)
            client = await _db.Clients.FirstAsync(c => c.IdClient == _idClient.Value);
        else
        {
            client = new Client { RegisteredAt = DateTimeDb.Now };
            _db.Clients.Add(client);
        }

        client.LastName = lastName!;
        client.FirstName = firstName!;
        client.Patronymic = string.IsNullOrWhiteSpace(patronymic) ? null : patronymic;
        client.Phone = phone;
        client.BirthDate = birthDate;

        await _db.SaveChangesAsync();
        Saved = true;
        Close();
    }
}
