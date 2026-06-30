using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Fitness_Club_01.Data;
using Fitness_Club_01.Models;
using Fitness_Club_01.Services;
using Fitness_Club_01.Views;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Controls;

public partial class GroupClassesControl : UserControl
{
    private readonly ApplicationDbContext _db = new();
    private readonly ObservableCollection<GroupClassRow> _rows = new();
    private readonly bool _trainerOnly;

    public GroupClassesControl(bool trainerOnly = false)
    {
        _trainerOnly = trainerOnly;
        InitializeComponent();
        ClassesGrid.ItemsSource = _rows;

        if (_trainerOnly)
        {
            HeaderText.Text = "Моё расписание";
            AddButton.IsVisible = false;
            EditButton.IsVisible = false;
        }

        Loaded += (_, _) => LoadData();
        AddButton.Click += async (_, _) => await OnAddAsync();
        EditButton.Click += async (_, _) => await OnEditAsync();
        RefreshButton.Click += (_, _) => LoadData();
        Unloaded += (_, _) => _db.Dispose();
    }

    private Window? GetOwner() => TopLevel.GetTopLevel(this) as Window;

    private void LoadData()
    {
        try
        {
            DbRefresh.ClearCache(_db);
            var query = _db.GroupClasses
                .Include(g => g.IdGymHallNavigation)
                .Include(g => g.IdTrainerNavigation)
                .Include(g => g.IdDayOfWeekNavigation)
                .AsNoTracking()
                .AsQueryable();

            if (_trainerOnly && Session.CurrentUser?.IdTrainer != null)
                query = query.Where(g => g.IdTrainer == Session.CurrentUser.IdTrainer);

            var list = query
                .OrderBy(g => g.IdDayOfWeekNavigation.DayNumber)
                .ThenBy(g => g.StartTime)
                .ToList();

            _rows.Clear();
            var n = 1;
            foreach (var g in list)
            {
                _rows.Add(new GroupClassRow
                {
                    IdGroupClass = g.IdGroupClass,
                    Number = n++,
                    ClassName = g.ClassName,
                    HallName = g.IdGymHallNavigation.HallName,
                    TrainerName = PersonNameFormatter.FullName(g.IdTrainerNavigation),
                    Schedule = $"{g.IdDayOfWeekNavigation.DayName} {g.StartTime:HH\\:mm}",
                    MaxParticipants = g.MaxParticipants.ToString(),
                    IsActive = g.IsActive ? "Да" : "Нет"
                });
            }

            StatusText.Text = $"Показано: {_rows.Count}";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Ошибка: {ex.Message}";
        }
    }

    private GroupClassRow? GetSelected() => ClassesGrid.SelectedItem as GroupClassRow;

    private async Task OnAddAsync()
    {
        var owner = GetOwner();
        if (owner == null) return;
        var win = new GroupClassEditWindow();
        await win.ShowDialog(owner);
        if (win.Saved) LoadData();
    }

    private async Task OnEditAsync()
    {
        var row = GetSelected();
        if (row == null)
        {
            StatusText.Text = "Выберите занятие";
            return;
        }

        var owner = GetOwner();
        if (owner == null) return;
        var win = new GroupClassEditWindow(row.IdGroupClass);
        await win.ShowDialog(owner);
        if (win.Saved) LoadData();
    }
}
