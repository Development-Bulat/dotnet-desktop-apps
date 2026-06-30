using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Fitness_Club_01.Data;
using Fitness_Club_01.Services;
using Fitness_Club_01.Views;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Controls;

public partial class ReferencesControl : UserControl
{
    private readonly ApplicationDbContext _db = new();
    private readonly ObservableCollection<RefRow> _halls = new();
    private readonly ObservableCollection<RefRow> _types = new();
    private readonly ObservableCollection<RefRow> _specs = new();

    public ReferencesControl()
    {
        InitializeComponent();
        HallsGrid.ItemsSource = _halls;
        TypesGrid.ItemsSource = _types;
        SpecsGrid.ItemsSource = _specs;

        Loaded += (_, _) => LoadAll();

        HallAddBtn.Click += async (_, _) => await EditRefAsync(ReferenceKind.GymHall);
        HallEditBtn.Click += async (_, _) =>
        {
            var id = GetSelectedId(HallsGrid);
            if (id == null) return;
            await EditRefAsync(ReferenceKind.GymHall, id);
        };
        HallDeleteBtn.Click += async (_, _) => await DeleteHallAsync();

        TypeAddBtn.Click += async (_, _) => await EditRefAsync(ReferenceKind.MembershipType);
        TypeEditBtn.Click += async (_, _) =>
        {
            var id = GetSelectedId(TypesGrid);
            if (id == null) return;
            await EditRefAsync(ReferenceKind.MembershipType, id);
        };
        TypeDeleteBtn.Click += async (_, _) => await DeleteTypeAsync();

        SpecAddBtn.Click += async (_, _) => await EditRefAsync(ReferenceKind.Specialization);
        SpecEditBtn.Click += async (_, _) =>
        {
            var id = GetSelectedId(SpecsGrid);
            if (id == null) return;
            await EditRefAsync(ReferenceKind.Specialization, id);
        };
        SpecDeleteBtn.Click += async (_, _) => await DeleteSpecAsync();

        Unloaded += (_, _) => _db.Dispose();
    }

    private Window? GetOwner() => TopLevel.GetTopLevel(this) as Window;

    private static int? GetSelectedId(DataGrid grid) =>
        (grid.SelectedItem as RefRow)?.Id;

    private void LoadAll()
    {
        DbRefresh.ClearCache(_db);

        _halls.Clear();
        foreach (var h in _db.GymHalls.OrderBy(x => x.HallName).ToList())
            _halls.Add(new RefRow(h.IdGymHall, h.HallName, h.Capacity.ToString()));

        _types.Clear();
        foreach (var t in _db.MembershipTypes.OrderBy(x => x.TypeName).ToList())
        {
            var limit = t.VisitLimit.HasValue ? $", {t.VisitLimit} виз." : ", безлимит";
            _types.Add(new RefRow(t.IdMembershipType, t.TypeName, $"{t.Price:N0} ₽, {t.DurationDays} дн.{limit}"));
        }

        _specs.Clear();
        foreach (var s in _db.Specializations.OrderBy(x => x.SpecializationName).ToList())
            _specs.Add(new RefRow(s.IdSpecialization, s.SpecializationName, ""));
    }

    private async Task EditRefAsync(ReferenceKind kind, int? id = null)
    {
        var owner = GetOwner();
        if (owner == null) return;

        if (id == null && kind != ReferenceKind.GymHall && kind != ReferenceKind.MembershipType && kind != ReferenceKind.Specialization)
            return;

        var win = new ReferenceEditWindow(kind, id);
        await win.ShowDialog(owner);
        if (win.Saved) LoadAll();
    }

    private async Task DeleteHallAsync()
    {
        var id = GetSelectedId(HallsGrid);
        if (id == null) return;
        var owner = GetOwner();
        if (owner == null) return;

        if (await MessageBox.Show(owner, "Удалить зал?", "Подтверждение", MessageBoxButtons.YesNo) != MessageBoxResult.Yes)
            return;

        try
        {
            var hall = await _db.GymHalls
                .Include(h => h.GroupClasses)
                .FirstOrDefaultAsync(h => h.IdGymHall == id);

            if (hall == null) return;

            if (hall.GroupClasses.Count > 0)
            {
                await MessageBox.Show(owner, "Нельзя удалить: зал используется в расписании занятий", "Ошибка", MessageBoxButtons.Ok);
                return;
            }

            _db.GymHalls.Remove(hall);
            await _db.SaveChangesAsync();
            LoadAll();
        }
        catch (Exception ex)
        {
            await MessageBox.Show(owner, $"Нельзя удалить зал: {ex.Message}", "Ошибка", MessageBoxButtons.Ok);
        }
    }

    private async Task DeleteTypeAsync()
    {
        var id = GetSelectedId(TypesGrid);
        if (id == null) return;
        var owner = GetOwner();
        if (owner == null) return;

        if (await MessageBox.Show(owner, "Удалить тариф?", "Подтверждение", MessageBoxButtons.YesNo) != MessageBoxResult.Yes)
            return;

        try
        {
            var type = await _db.MembershipTypes
                .Include(t => t.Memberships)
                .FirstOrDefaultAsync(t => t.IdMembershipType == id);

            if (type == null) return;

            if (type.Memberships.Count > 0)
            {
                await MessageBox.Show(owner, "Нельзя удалить: по этому тарифу уже оформлены абонементы", "Ошибка", MessageBoxButtons.Ok);
                return;
            }

            _db.MembershipTypes.Remove(type);
            await _db.SaveChangesAsync();
            LoadAll();
        }
        catch (Exception ex)
        {
            await MessageBox.Show(owner, $"Нельзя удалить тариф: {ex.Message}", "Ошибка", MessageBoxButtons.Ok);
        }
    }

    private async Task DeleteSpecAsync()
    {
        var id = GetSelectedId(SpecsGrid);
        if (id == null) return;
        var owner = GetOwner();
        if (owner == null) return;

        if (await MessageBox.Show(owner, "Удалить специализацию?", "Подтверждение", MessageBoxButtons.YesNo) != MessageBoxResult.Yes)
            return;

        try
        {
            var spec = await _db.Specializations.FindAsync(id);
            if (spec == null) return;

            _db.Specializations.Remove(spec);
            await _db.SaveChangesAsync();
            LoadAll();
        }
        catch (Exception ex)
        {
            await MessageBox.Show(owner, $"Нельзя удалить специализацию: {ex.Message}", "Ошибка", MessageBoxButtons.Ok);
        }
    }

    private sealed record RefRow(int Id, string Name, string Extra);
}
