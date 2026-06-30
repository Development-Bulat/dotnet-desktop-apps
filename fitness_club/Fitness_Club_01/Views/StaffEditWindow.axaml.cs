using Avalonia.Controls;
using Avalonia.Interactivity;
using Fitness_Club_01.Data;
using Fitness_Club_01.Services;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Views;

public partial class StaffEditWindow : Window
{
    private readonly ApplicationDbContext _db = new();
    private readonly int? _idStaff;

    public bool Saved { get; private set; }

    public StaffEditWindow(int? idStaff = null)
    {
        _idStaff = idStaff;
        InitializeComponent();
        PhoneInputFormatter.Attach(PhoneBox);
        SaveButton.Click += OnSaveClick;
        CancelButton.Click += (_, _) => Close();
        Loaded += async (_, _) => await LoadAsync();
        Closed += (_, _) => _db.Dispose();
    }

    private async Task LoadAsync()
    {
        HiredDatePicker.SelectedDate = DateTime.Today;

        if (_idStaff == null) return;

        TitleText.Text = "Редактирование сотрудника";
        var staff = await _db.Staff.FindAsync(_idStaff);
        if (staff == null) return;

        LastNameBox.Text = staff.LastName;
        FirstNameBox.Text = staff.FirstName;
        PatronymicBox.Text = staff.Patronymic;
        PhoneInputFormatter.SetFromNormalized(PhoneBox, staff.Phone);
        HiredDatePicker.SelectedDate = staff.HiredAt.ToDateTime(TimeOnly.MinValue);
    }

    private async void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        var lastName = LastNameBox.Text?.Trim();
        var firstName = FirstNameBox.Text?.Trim();
        var patronymic = PatronymicBox.Text?.Trim();

        var lastNameError = NameValidator.ValidateRequired(lastName, "Фамилию");
        if (lastNameError != null) { ErrorText.Text = lastNameError; return; }

        var firstNameError = NameValidator.ValidateRequired(firstName, "Имя");
        if (firstNameError != null) { ErrorText.Text = firstNameError; return; }

        var patronymicError = NameValidator.ValidateOptional(patronymic, "Отчество");
        if (patronymicError != null) { ErrorText.Text = patronymicError; return; }

        if (!HiredDatePicker.SelectedDate.HasValue)
        {
            ErrorText.Text = "Укажите дату приёма";
            return;
        }

        var hiredAt = DateOnly.FromDateTime(HiredDatePicker.SelectedDate.Value.DateTime);

        var phoneError = PhoneValidator.ValidateOptionalAndNormalize(PhoneBox.Text?.Trim(), out var phone);
        if (phoneError != null) { ErrorText.Text = phoneError; return; }

        if (phone != null && await _db.Staff.AnyAsync(s => s.Phone == phone && s.IdStaff != (_idStaff ?? 0)))
        {
            ErrorText.Text = "Телефон уже используется";
            return;
        }

        Staff staff;
        if (_idStaff.HasValue)
            staff = await _db.Staff.FirstAsync(s => s.IdStaff == _idStaff.Value);
        else
        {
            staff = new Staff();
            _db.Staff.Add(staff);
        }

        staff.LastName = lastName!;
        staff.FirstName = firstName!;
        staff.Patronymic = string.IsNullOrWhiteSpace(patronymic) ? null : patronymic;
        staff.Phone = phone;
        staff.HiredAt = hiredAt;

        await _db.SaveChangesAsync();
        Saved = true;
        Close();
    }
}
