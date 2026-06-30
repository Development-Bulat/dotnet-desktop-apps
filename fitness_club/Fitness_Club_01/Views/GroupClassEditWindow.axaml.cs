using Avalonia.Controls;
using Avalonia.Interactivity;
using Fitness_Club_01.Data;
using Fitness_Club_01.Services;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Views;

public partial class GroupClassEditWindow : Window
{
    private readonly ApplicationDbContext _db = new();
    private readonly int? _idGroupClass;

    public bool Saved { get; private set; }

    public GroupClassEditWindow(int? idGroupClass = null)
    {
        _idGroupClass = idGroupClass;
        InitializeComponent();
        SaveButton.Click += OnSaveClick;
        CancelButton.Click += (_, _) => Close();
        Loaded += async (_, _) => await LoadAsync();
        Closed += (_, _) => _db.Dispose();
    }

    private async Task LoadAsync()
    {
        var halls = await _db.GymHalls.OrderBy(h => h.HallName).ToListAsync();
        var trainers = await _db.Trainers.OrderBy(t => t.LastName).ToListAsync();
        var days = await _db.DayOfWeeks.OrderBy(d => d.DayNumber).ToListAsync();

        HallCombo.ItemsSource = halls.Select(h => new ComboItem(h.IdGymHall, h.HallName)).ToList();
        TrainerCombo.ItemsSource = trainers.Select(t => new ComboItem(t.IdTrainer, PersonNameFormatter.FullName(t))).ToList();
        DayCombo.ItemsSource = days.Select(d => new ComboItem(d.IdDayOfWeek, d.DayName)).ToList();

        if (_idGroupClass == null)
        {
            DurationBox.Text = "60";
            MaxParticipantsBox.Text = "15";
            return;
        }

        TitleText.Text = "Редактирование занятия";
        var gc = await _db.GroupClasses.FindAsync(_idGroupClass);
        if (gc == null) return;

        ClassNameBox.Text = gc.ClassName;
        HallCombo.SelectedItem = ((List<ComboItem>)HallCombo.ItemsSource!).First(i => i.Id == gc.IdGymHall);
        TrainerCombo.SelectedItem = ((List<ComboItem>)TrainerCombo.ItemsSource!).First(i => i.Id == gc.IdTrainer);
        DayCombo.SelectedItem = ((List<ComboItem>)DayCombo.ItemsSource!).First(i => i.Id == gc.IdDayOfWeek);
        StartTimeBox.Text = gc.StartTime.ToString("HH:mm");
        DurationBox.Text = gc.DurationMinutes.ToString();
        MaxParticipantsBox.Text = gc.MaxParticipants.ToString();
        IsActiveCheck.IsChecked = gc.IsActive;
    }

    private async void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        var className = ClassNameBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(className))
        {
            ErrorText.Text = "Укажите название";
            return;
        }

        if (HallCombo.SelectedItem is not ComboItem hallItem ||
            TrainerCombo.SelectedItem is not ComboItem trainerItem ||
            DayCombo.SelectedItem is not ComboItem dayItem)
        {
            ErrorText.Text = "Заполните все поля";
            return;
        }

        if (!TimeOnly.TryParse(StartTimeBox.Text?.Trim(), out var startTime))
        {
            ErrorText.Text = "Некорректное время (формат ЧЧ:ММ)";
            return;
        }

        if (!int.TryParse(DurationBox.Text?.Trim(), out var duration) || duration <= 0)
        {
            ErrorText.Text = "Укажите длительность в минутах";
            return;
        }

        if (!int.TryParse(MaxParticipantsBox.Text?.Trim(), out var maxParticipants) || maxParticipants <= 0)
        {
            ErrorText.Text = "Укажите максимум участников";
            return;
        }

        GroupClass entity;
        if (_idGroupClass.HasValue)
            entity = await _db.GroupClasses.FirstAsync(g => g.IdGroupClass == _idGroupClass.Value);
        else
        {
            entity = new GroupClass();
            _db.GroupClasses.Add(entity);
        }

        entity.ClassName = className;
        entity.IdGymHall = hallItem.Id;
        entity.IdTrainer = trainerItem.Id;
        entity.IdDayOfWeek = dayItem.Id;
        entity.StartTime = startTime;
        entity.DurationMinutes = duration;
        entity.MaxParticipants = maxParticipants;
        entity.IsActive = IsActiveCheck.IsChecked == true;

        var scheduleError = await ScheduleValidator.ValidateGroupClassScheduleAsync(
            _db,
            hallItem.Id,
            dayItem.Id,
            startTime,
            duration,
            maxParticipants,
            entity.IsActive,
            _idGroupClass);

        if (scheduleError != null)
        {
            ErrorText.Text = scheduleError;
            return;
        }

        await _db.SaveChangesAsync();
        Saved = true;
        Close();
    }

    private sealed record ComboItem(int Id, string Title)
    {
        public override string ToString() => Title;
    }
}
