using Avalonia.Controls;
using Avalonia.Interactivity;
using Fitness_Club_01.Data;
using Fitness_Club_01.Services;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Views;

public partial class TrainerEditWindow : Window
{
    private readonly ApplicationDbContext _db = new();
    private readonly int? _idTrainer;
    private List<Specialization> _specializations = new();

    public bool Saved { get; private set; }

    public TrainerEditWindow(int? idTrainer = null)
    {
        _idTrainer = idTrainer;
        InitializeComponent();
        PhoneInputFormatter.Attach(PhoneBox);
        SaveButton.Click += OnSaveClick;
        CancelButton.Click += (_, _) => Close();
        Loaded += async (_, _) => await LoadAsync();
        Closed += (_, _) => _db.Dispose();
    }

    private async Task LoadAsync()
    {
        _specializations = await _db.Specializations.OrderBy(s => s.SpecializationName).ToListAsync();
        SpecializationsList.ItemsSource = _specializations;
        HiredDatePicker.SelectedDate = DateTime.Today;

        if (_idTrainer == null) return;

        TitleText.Text = "Редактирование тренера";
        var trainer = await _db.Trainers
            .Include(t => t.IdSpecializations)
            .FirstOrDefaultAsync(t => t.IdTrainer == _idTrainer);

        if (trainer == null) return;

        LastNameBox.Text = trainer.LastName;
        FirstNameBox.Text = trainer.FirstName;
        PatronymicBox.Text = trainer.Patronymic;
        PhoneInputFormatter.SetFromNormalized(PhoneBox, trainer.Phone);
        HiredDatePicker.SelectedDate = trainer.HiredAt.ToDateTime(TimeOnly.MinValue);

        foreach (var spec in trainer.IdSpecializations)
        {
            var item = _specializations.FirstOrDefault(s => s.IdSpecialization == spec.IdSpecialization);
            if (item != null)
                SpecializationsList.SelectedItems.Add(item);
        }
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

        if (phone != null && await _db.Trainers.AnyAsync(t => t.Phone == phone && t.IdTrainer != (_idTrainer ?? 0)))
        {
            ErrorText.Text = "Телефон уже используется";
            return;
        }

        Trainer trainer;
        if (_idTrainer.HasValue)
        {
            trainer = await _db.Trainers
                .Include(t => t.IdSpecializations)
                .FirstAsync(t => t.IdTrainer == _idTrainer.Value);
            trainer.IdSpecializations.Clear();
        }
        else
        {
            trainer = new Trainer();
            _db.Trainers.Add(trainer);
        }

        trainer.LastName = lastName!;
        trainer.FirstName = firstName!;
        trainer.Patronymic = string.IsNullOrWhiteSpace(patronymic) ? null : patronymic;
        trainer.Phone = phone;
        trainer.HiredAt = hiredAt;

        foreach (var selected in SpecializationsList.SelectedItems.Cast<Specialization>())
            trainer.IdSpecializations.Add(selected);

        await _db.SaveChangesAsync();
        Saved = true;
        Close();
    }
}
