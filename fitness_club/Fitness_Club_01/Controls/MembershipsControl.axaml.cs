using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Fitness_Club_01.Data;
using Fitness_Club_01.Models;
using Fitness_Club_01.Services;
using Fitness_Club_01.Views;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Controls;

public partial class MembershipsControl : UserControl
{
    private readonly ApplicationDbContext _db = new();
    private readonly ObservableCollection<MembershipRow> _rows = new();
    private readonly bool _clientOnly;

    public MembershipsControl(bool clientOnly = false)
    {
        _clientOnly = clientOnly;
        InitializeComponent();
        MembershipsGrid.ItemsSource = _rows;

        if (_clientOnly)
        {
            HeaderText.Text = "Мой абонемент";
            AddButton.Content = "Купить абонемент";
            AddButton.MinWidth = 180;
            StatusButton.IsVisible = false;
            SearchBox.IsVisible = false;
            StatusFilter.IsVisible = false;
            DataGridHelper.RemoveColumn(MembershipsGrid, 1);
        }
        else if (Session.IsAdmin)
        {
            AddButton.IsVisible = false;
        }

        Loaded += async (_, _) =>
        {
            await LoadStatusFilterAsync();
            LoadData();
        };
        SearchBox.TextChanged += (_, _) => LoadData();
        StatusFilter.SelectionChanged += (_, _) => LoadData();
        AddButton.Click += async (_, _) => await OnAddAsync();
        StatusButton.Click += async (_, _) => await OnStatusAsync();
        RefreshButton.Click += (_, _) => LoadData();
        Unloaded += (_, _) => _db.Dispose();
    }

    private Window? GetOwner() => TopLevel.GetTopLevel(this) as Window;

    private async void LoadData()
    {
        try
        {
            await MembershipService.ExpireOutdatedAsync(_db);
            DbRefresh.ClearCache(_db);
            var search = SearchBox.Text?.Trim().ToLower() ?? "";
            var statusFilter = (StatusFilter.SelectedItem as ComboItem)?.Code;

            var query = _db.Memberships
                .Include(m => m.IdClientNavigation)
                .Include(m => m.IdMembershipTypeNavigation)
                .Include(m => m.IdMembershipStatusNavigation)
                .AsNoTracking()
                .AsQueryable();

            if (_clientOnly && Session.CurrentUser?.IdClient != null)
                query = query.Where(m => m.IdClient == Session.CurrentUser.IdClient);

            if (!string.IsNullOrEmpty(statusFilter))
            {
                var status = await _db.MembershipStatuses.FirstAsync(s => s.StatusCode == statusFilter);
                query = query.Where(m => m.IdMembershipStatus == status.IdMembershipStatus);
            }

            var list = query
                .OrderByDescending(m => m.StartDate)
                .ToList();

            if (!string.IsNullOrEmpty(search))
            {
                list = list.Where(m =>
                    PersonNameFormatter.FullName(m.IdClientNavigation).ToLower().Contains(search) ||
                    m.IdMembershipTypeNavigation.TypeName.ToLower().Contains(search)).ToList();
            }

            _rows.Clear();
            var n = 1;
            foreach (var m in list)
            {
                _rows.Add(new MembershipRow
                {
                    IdMembership = m.IdMembership,
                    Number = n++,
                    IdClient = m.IdClient,
                    ClientName = PersonNameFormatter.FullName(m.IdClientNavigation),
                    TypeName = m.IdMembershipTypeNavigation.TypeName,
                    StatusName = MembershipService.GetEffectiveStatusName(
                        m,
                        m.IdMembershipStatusNavigation.StatusCode,
                        m.IdMembershipStatusNavigation.StatusName),
                    StatusCode = m.IdMembershipStatusNavigation.StatusCode,
                    Period = $"{m.StartDate:dd.MM.yyyy} — {m.EndDate:dd.MM.yyyy}",
                    Price = $"{m.IdMembershipTypeNavigation.Price:N0} ₽"
                });
            }

            StatusText.Text = $"Показано: {_rows.Count}";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Ошибка: {ex.Message}";
        }
    }

    private MembershipRow? GetSelected() => MembershipsGrid.SelectedItem as MembershipRow;

    private async Task OnAddAsync()
    {
        var owner = GetOwner();
        if (owner == null) return;

        if (_clientOnly)
        {
            if (Session.CurrentUser?.IdClient == null)
            {
                StatusText.Text = "Не удалось определить клиента";
                return;
            }

            var win = new MembershipEditWindow(Session.CurrentUser.IdClient);
            await win.ShowDialog(owner);
            if (win.Saved) LoadData();
            return;
        }

        if (Session.IsAdmin)
        {
            StatusText.Text = "Оформление абонемента доступно только на ресепшене";
            return;
        }

        var staffWin = new MembershipEditWindow();
        await staffWin.ShowDialog(owner);
        if (staffWin.Saved) LoadData();
    }

    private async Task OnStatusAsync()
    {
        var row = GetSelected();
        if (row == null)
        {
            StatusText.Text = "Выберите абонемент";
            return;
        }

        var owner = GetOwner();
        if (owner == null) return;

        var win = new MembershipStatusWindow(row.IdMembership);
        await win.ShowDialog(owner);
        if (win.Changed) LoadData();
    }

    private sealed record ComboItem(string Code, string Title)
    {
        public override string ToString() => Title;
    }

    private async Task LoadStatusFilterAsync()
    {
        var statuses = await _db.MembershipStatuses.OrderBy(s => s.StatusName).ToListAsync();
        StatusFilter.ItemsSource = new List<ComboItem>
        {
            new("", "Все статусы")
        }.Concat(statuses.Select(s => new ComboItem(s.StatusCode, s.StatusName))).ToList();
        StatusFilter.SelectedIndex = 0;
    }
}
