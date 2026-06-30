using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Fitness_Club_01.Data;
using Fitness_Club_01.Models;
using Fitness_Club_01.Services;
using Fitness_Club_01.Views;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Controls;

public partial class TrainersControl : UserControl
{
    private readonly ApplicationDbContext _db = new();
    private readonly ObservableCollection<TrainerRow> _rows = new();

    public TrainersControl()
    {
        InitializeComponent();
        TrainersGrid.ItemsSource = _rows;
        Loaded += (_, _) => LoadData();
        AddButton.Click += async (_, _) => await OnAddAsync();
        EditButton.Click += async (_, _) => await OnEditAsync();
        AccountButton.Click += async (_, _) => await OnAccountAsync();
        DeleteButton.Click += async (_, _) => await OnDeleteAsync();
        RefreshButton.Click += (_, _) => LoadData();
        Unloaded += (_, _) => _db.Dispose();
    }

    private Window? GetOwner() => TopLevel.GetTopLevel(this) as Window;

    private void LoadData()
    {
        try
        {
            DbRefresh.ClearCache(_db);
            var list = _db.Trainers
                .Include(t => t.IdSpecializations)
                .Include(t => t.UserAccounts)
                .AsNoTracking()
                .OrderBy(t => t.LastName)
                .ToList();

            _rows.Clear();
            var n = 1;
            foreach (var t in list)
            {
                var account = t.UserAccounts.FirstOrDefault();
                _rows.Add(new TrainerRow
                {
                    IdTrainer = t.IdTrainer,
                    Number = n++,
                    FullName = PersonNameFormatter.FullName(t),
                    Phone = PhoneInputFormatter.FormatStored(t.Phone),
                    Specializations = string.Join(", ", t.IdSpecializations.Select(s => s.SpecializationName)),
                    HiredAtText = t.HiredAt.ToString("dd.MM.yyyy"),
                    HasAccount = account != null ? "Да" : "Нет",
                    Login = account?.Login ?? "—"
                });
            }

            StatusText.Text = $"Показано: {_rows.Count}";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Ошибка: {ex.Message}";
        }
    }

    private TrainerRow? GetSelected() => TrainersGrid.SelectedItem as TrainerRow;

    private async Task OnAddAsync()
    {
        var owner = GetOwner();
        if (owner == null) return;
        var win = new TrainerEditWindow();
        await win.ShowDialog(owner);
        if (win.Saved) LoadData();
    }

    private async Task OnEditAsync()
    {
        var row = GetSelected();
        if (row == null) { StatusText.Text = "Выберите тренера"; return; }
        var owner = GetOwner();
        if (owner == null) return;
        var win = new TrainerEditWindow(row.IdTrainer);
        await win.ShowDialog(owner);
        if (win.Saved) LoadData();
    }

    private async Task OnAccountAsync()
    {
        var row = GetSelected();
        if (row == null) { StatusText.Text = "Выберите тренера"; return; }
        if (row.HasAccount == "Да") { StatusText.Text = "Уже есть учётная запись"; return; }
        var owner = GetOwner();
        if (owner == null) return;
        var win = new TrainerAccountWindow(row.IdTrainer);
        await win.ShowDialog(owner);
        if (win.Saved) LoadData();
    }

    private async Task OnDeleteAsync()
    {
        var row = GetSelected();
        if (row == null) { StatusText.Text = "Выберите тренера"; return; }
        var owner = GetOwner();
        if (owner == null) return;

        if (await MessageBox.Show(owner, $"Удалить тренера {row.FullName}?", "Подтверждение", MessageBoxButtons.YesNo)
            != MessageBoxResult.Yes)
            return;

        try
        {
            var trainer = await _db.Trainers
                .Include(t => t.UserAccounts)
                .Include(t => t.GroupClasses)
                .FirstOrDefaultAsync(t => t.IdTrainer == row.IdTrainer);

            if (trainer == null) return;

            if (trainer.GroupClasses.Count > 0)
            {
                StatusText.Text = "Нельзя удалить: тренер ведёт занятия";
                return;
            }

            foreach (var account in trainer.UserAccounts.ToList())
                _db.UserAccounts.Remove(account);

            _db.Trainers.Remove(trainer);
            await _db.SaveChangesAsync();
            LoadData();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Ошибка: {ex.Message}";
        }
    }
}
