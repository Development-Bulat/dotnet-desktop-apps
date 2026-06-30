using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Fitness_Club_01.Data;
using Fitness_Club_01.Models;
using Fitness_Club_01.Services;

namespace Fitness_Club_01.Controls;

public partial class TrainerClassSessionsControl : UserControl
{
    private readonly ApplicationDbContext _db = new();
    private readonly ObservableCollection<ClassSessionRow> _rows = new();
    private int _year = DateTime.Today.Year;
    private int _month = DateTime.Today.Month;

    public TrainerClassSessionsControl()
    {
        InitializeComponent();
        SessionsGrid.ItemsSource = _rows;

        Loaded += async (_, _) => await LoadDataAsync();
        PrevMonthButton.Click += async (_, _) => await ChangeMonthAsync(-1);
        NextMonthButton.Click += async (_, _) => await ChangeMonthAsync(1);
        CancelSessionButton.Click += async (_, _) => await OnCancelSessionAsync();
        RefreshButton.Click += async (_, _) => await LoadDataAsync();
        Unloaded += (_, _) => _db.Dispose();
    }

    private Window? GetOwner() => TopLevel.GetTopLevel(this) as Window;

    private async Task ChangeMonthAsync(int delta)
    {
        var date = new DateOnly(_year, _month, 1).AddMonths(delta);
        _year = date.Year;
        _month = date.Month;
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            if (Session.CurrentUser?.IdTrainer == null)
            {
                StatusText.Text = "Пользователь не привязан к тренеру";
                return;
            }

            DbRefresh.ClearCache(_db);
            MonthText.Text = new DateTime(_year, _month, 1)
                .ToString("MMMM yyyy", CultureInfo.GetCultureInfo("ru-RU"));

            var list = await ClassSessionService.GetMonthlySessionsAsync(
                _db,
                Session.CurrentUser.IdTrainer.Value,
                _year,
                _month);

            _rows.Clear();
            foreach (var row in list)
                _rows.Add(row);

            StatusText.Text = $"Показано: {_rows.Count} занятий";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Ошибка: {ex.Message}";
        }
    }

    private ClassSessionRow? GetSelected() => SessionsGrid.SelectedItem as ClassSessionRow;

    private async Task OnCancelSessionAsync()
    {
        if (Session.CurrentUser?.IdTrainer == null)
            return;

        var row = GetSelected();
        if (row == null)
        {
            StatusText.Text = "Выберите занятие";
            return;
        }

        if (row.IsCancelled)
        {
            StatusText.Text = "Занятие уже отменено";
            return;
        }

        if (!row.CanCancel)
        {
            StatusText.Text = "Нельзя отменить прошедшее занятие";
            return;
        }

        var owner = GetOwner();
        if (owner == null)
            return;

        var confirmText = row.BookedCount.StartsWith("0/")
            ? $"Отменить занятие «{row.ClassName}» {row.ClassDate} в {row.ClassTime}?"
            : $"Отменить занятие «{row.ClassName}» {row.ClassDate} в {row.ClassTime}?\nВсе записи клиентов будут удалены, им придёт уведомление.";

        if (await MessageBox.Show(owner, confirmText, "Подтверждение", MessageBoxButtons.YesNo)
            != MessageBoxResult.Yes)
            return;

        var error = await ClassSessionService.CancelSessionAsync(
            _db,
            Session.CurrentUser.IdTrainer.Value,
            row.IdGroupClass,
            row.ClassDateValue,
            Session.CurrentUser);

        if (error != null)
        {
            StatusText.Text = error;
            return;
        }

        StatusText.Text = "Занятие отменено";
        await LoadDataAsync();
    }
}
