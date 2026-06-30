using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Fitness_Club_01.Data;
using Fitness_Club_01.Models;
using Fitness_Club_01.Services;
using Fitness_Club_01.Views;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Controls;

public partial class StaffControl : UserControl
{
    private readonly ApplicationDbContext _db = new();
    private readonly ObservableCollection<StaffRow> _rows = new();

    public StaffControl()
    {
        InitializeComponent();
        StaffGrid.ItemsSource = _rows;
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
            var list = _db.Staff
                .Include(s => s.UserAccounts)
                    .ThenInclude(u => u.IdUserRoleNavigation)
                .AsNoTracking()
                .OrderBy(s => s.LastName)
                .ToList();

            _rows.Clear();
            var n = 1;
            foreach (var s in list)
            {
                var account = s.UserAccounts.FirstOrDefault();
                _rows.Add(new StaffRow
                {
                    IdStaff = s.IdStaff,
                    Number = n++,
                    FullName = PersonNameFormatter.FullName(s),
                    Phone = PhoneInputFormatter.FormatStored(s.Phone),
                    HiredAtText = s.HiredAt.ToString("dd.MM.yyyy"),
                    HasAccount = account != null ? "Да" : "Нет",
                    Login = account?.Login ?? "—",
                    RoleName = account?.IdUserRoleNavigation.RoleName ?? "—"
                });
            }

            StatusText.Text = $"Показано: {_rows.Count}";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Ошибка: {ex.Message}";
        }
    }

    private StaffRow? GetSelected() => StaffGrid.SelectedItem as StaffRow;

    private async Task OnAddAsync()
    {
        var owner = GetOwner();
        if (owner == null) return;
        var win = new StaffEditWindow();
        await win.ShowDialog(owner);
        if (win.Saved) LoadData();
    }

    private async Task OnEditAsync()
    {
        var row = GetSelected();
        if (row == null) { StatusText.Text = "Выберите сотрудника"; return; }
        var owner = GetOwner();
        if (owner == null) return;
        var win = new StaffEditWindow(row.IdStaff);
        await win.ShowDialog(owner);
        if (win.Saved) LoadData();
    }

    private async Task OnAccountAsync()
    {
        var row = GetSelected();
        if (row == null) { StatusText.Text = "Выберите сотрудника"; return; }
        if (row.HasAccount == "Да") { StatusText.Text = "Уже есть учётная запись"; return; }
        var owner = GetOwner();
        if (owner == null) return;
        var win = new StaffAccountWindow(row.IdStaff);
        await win.ShowDialog(owner);
        if (win.Saved) LoadData();
    }

    private async Task OnDeleteAsync()
    {
        var row = GetSelected();
        if (row == null) { StatusText.Text = "Выберите сотрудника"; return; }
        var owner = GetOwner();
        if (owner == null) return;

        if (await MessageBox.Show(owner, $"Удалить сотрудника {row.FullName}?", "Подтверждение", MessageBoxButtons.YesNo)
            != MessageBoxResult.Yes)
            return;

        var staff = await _db.Staff.Include(s => s.UserAccounts).FirstOrDefaultAsync(s => s.IdStaff == row.IdStaff);
        if (staff == null) return;

        foreach (var account in staff.UserAccounts.ToList())
            _db.UserAccounts.Remove(account);

        _db.Staff.Remove(staff);
        await _db.SaveChangesAsync();
        LoadData();
    }
}
