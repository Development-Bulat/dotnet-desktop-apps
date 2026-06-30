using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Fitness_Club_01.Data;
using Fitness_Club_01.Models;
using Fitness_Club_01.Services;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Controls;

public partial class VisitsControl : UserControl
{
    private readonly ApplicationDbContext _db = new();
    private readonly ObservableCollection<ExpectedVisitRow> _expectedRows = new();
    private readonly ObservableCollection<GymExpectedRow> _gymExpectedRows = new();
    private readonly ObservableCollection<VisitRow> _visitRows = new();
    private readonly bool _canMark;

    public VisitsControl()
    {
        InitializeComponent();
        ExpectedGrid.ItemsSource = _expectedRows;
        GymExpectedGrid.ItemsSource = _gymExpectedRows;
        VisitsGrid.ItemsSource = _visitRows;
        _canMark = Session.IsReception;

        MarkClassArrivalButton.IsVisible = _canMark;
        MarkGymArrivalButton.IsVisible = _canMark;

        Loaded += async (_, _) =>
        {
            DayDatePicker.SelectedDate = DateTime.Today;
            await LoadDataAsync();
        };
        DayDatePicker.SelectedDateChanged += async (_, _) => await LoadDataAsync();
        SearchBox.TextChanged += async (_, _) => await LoadVisitsForDayAsync();
        MarkClassArrivalButton.Click += async (_, _) => await OnMarkClassArrivalAsync();
        MarkGymArrivalButton.Click += async (_, _) => await OnMarkGymArrivalAsync();
        RefreshButton.Click += async (_, _) => await LoadDataAsync();
        Unloaded += (_, _) => _db.Dispose();
    }

    private DateOnly GetSelectedDate() =>
        DayDatePicker.SelectedDate.HasValue
            ? DateOnly.FromDateTime(DayDatePicker.SelectedDate.Value)
            : DateOnly.FromDateTime(DateTime.Today);

    private bool CanMarkToday() =>
        _canMark && GetSelectedDate() == DateOnly.FromDateTime(DateTime.Today);

    private async Task LoadDataAsync()
    {
        try
        {
            DbRefresh.ClearCache(_db);
            await MembershipService.ExpireOutdatedAsync(_db);

            var (current, capacity) = await VisitService.GetClubOccupancyAsync(_db);
            if (capacity > 0)
            {
                OccupancyText.Text = CanMarkToday() && current >= capacity
                    ? $"В клубе: {current}/{capacity} — мест нет, подождите ~1 ч."
                    : $"В клубе сейчас: {current}/{capacity}";
                OccupancyText.Foreground = CanMarkToday() && current >= capacity
                    ? new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#EF4444"))
                    : new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#0F172A"));
            }
            else
            {
                OccupancyText.Text = "";
            }

            await LoadExpectedAsync();
            await LoadGymExpectedAsync();
            await LoadVisitsForDayAsync();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Ошибка: {ex.Message}";
        }
    }

    private async Task<HashSet<int>> GetVisitedClientIdsAsync(DateOnly day)
    {
        var ids = await _db.Visits
            .AsNoTracking()
            .Where(v => DateOnly.FromDateTime(v.VisitDateTime) == day)
            .Select(v => v.IdClient)
            .ToListAsync();

        return ids.ToHashSet();
    }

    private async Task LoadExpectedAsync()
    {
        var day = GetSelectedDate();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var canMark = CanMarkToday();

        var bookings = await _db.ClassBookings
            .Include(b => b.IdClientNavigation)
            .Include(b => b.IdGroupClassNavigation)
                .ThenInclude(g => g.IdGymHallNavigation)
            .AsNoTracking()
            .Where(b => b.ClassDate == day)
            .OrderBy(b => b.IdGroupClassNavigation.StartTime)
            .ThenBy(b => b.IdClientNavigation.LastName)
            .ToListAsync();

        var cancelled = await _db.CancelledClassSessions
            .AsNoTracking()
            .Where(c => c.ClassDate == day)
            .Select(c => new { c.IdGroupClass, c.ClassDate })
            .ToListAsync();

        var cancelledSet = cancelled.Select(c => (c.IdGroupClass, c.ClassDate)).ToHashSet();
        var visitedSet = await GetVisitedClientIdsAsync(day);

        _expectedRows.Clear();
        var n = 1;
        foreach (var b in bookings)
        {
            if (cancelledSet.Contains((b.IdGroupClass, b.ClassDate)))
                continue;

            var visited = visitedSet.Contains(b.IdClient);
            _expectedRows.Add(new ExpectedVisitRow
            {
                IdClient = b.IdClient,
                IdClassBooking = b.IdClassBooking,
                Number = n++,
                ClientName = PersonNameFormatter.FullName(b.IdClientNavigation),
                VisitType = "Занятие",
                ClassName = b.IdGroupClassNavigation.ClassName,
                ClassTime = b.IdGroupClassNavigation.StartTime.ToString("HH:mm"),
                HallName = b.IdGroupClassNavigation.IdGymHallNavigation.HallName,
                Status = visited ? "Пришёл" : day < today ? "Не пришёл" : "Ожидается",
                CanMark = canMark && !visited
            });
        }
    }

    private async Task LoadGymExpectedAsync()
    {
        var day = GetSelectedDate();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var canMark = CanMarkToday();

        var activeStatus = await _db.MembershipStatuses
            .AsNoTracking()
            .FirstAsync(s => s.StatusCode == "active");

        var memberships = await _db.Memberships
            .Include(m => m.IdClientNavigation)
            .Include(m => m.IdMembershipTypeNavigation)
            .AsNoTracking()
            .Where(m => m.IdMembershipStatus == activeStatus.IdMembershipStatus
                        && m.StartDate <= day
                        && m.EndDate >= day)
            .OrderBy(m => m.IdClientNavigation.LastName)
            .ThenBy(m => m.IdClientNavigation.FirstName)
            .ToListAsync();

        var clientsWithClassBooking = await _db.ClassBookings
            .AsNoTracking()
            .Where(b => b.ClassDate == day)
            .Select(b => b.IdClient)
            .Distinct()
            .ToListAsync();

        var bookingSet = clientsWithClassBooking.ToHashSet();
        var visitedSet = await GetVisitedClientIdsAsync(day);

        var seenClients = new HashSet<int>();
        _gymExpectedRows.Clear();
        var n = 1;

        foreach (var membership in memberships)
        {
            if (!seenClients.Add(membership.IdClient))
                continue;

            if (bookingSet.Contains(membership.IdClient))
                continue;

            var visited = visitedSet.Contains(membership.IdClient);
            _gymExpectedRows.Add(new GymExpectedRow
            {
                IdClient = membership.IdClient,
                Number = n++,
                ClientName = PersonNameFormatter.FullName(membership.IdClientNavigation),
                MembershipType = membership.IdMembershipTypeNavigation.TypeName,
                Status = visited ? "Пришёл" : day < today ? "Не пришёл" : "Может посетить зал",
                CanMark = canMark && !visited
            });
        }

        UpdateStatusText();
    }

    private async Task LoadVisitsForDayAsync()
    {
        try
        {
            var day = GetSelectedDate();
            var search = SearchBox.Text?.Trim().ToLower() ?? "";

            var visits = await _db.Visits
                .Include(v => v.IdClientNavigation)
                .Include(v => v.IdMarkedByUserNavigation)
                .Include(v => v.IdMembershipNavigation)
                    .ThenInclude(m => m!.IdMembershipTypeNavigation)
                .AsNoTracking()
                .Where(v => DateOnly.FromDateTime(v.VisitDateTime) == day)
                .OrderByDescending(v => v.VisitDateTime)
                .ToListAsync();

            var bookings = await _db.ClassBookings
                .Include(b => b.IdGroupClassNavigation)
                .AsNoTracking()
                .Where(b => b.ClassDate == day)
                .ToListAsync();

            var bookingByClient = bookings
                .GroupBy(b => b.IdClient)
                .ToDictionary(g => g.Key, g => g.First().IdGroupClassNavigation.ClassName);

            if (!string.IsNullOrEmpty(search))
            {
                visits = visits.Where(v =>
                    PersonNameFormatter.FullName(v.IdClientNavigation).ToLower().Contains(search)).ToList();
            }

            _visitRows.Clear();
            var n = 1;
            foreach (var v in visits)
            {
                var visitType = bookingByClient.TryGetValue(v.IdClient, out var className)
                    ? className
                    : "Тренажёрный зал";

                _visitRows.Add(new VisitRow
                {
                    IdVisit = v.IdVisit,
                    Number = n++,
                    ClientName = PersonNameFormatter.FullName(v.IdClientNavigation),
                    VisitType = visitType,
                    VisitDateTime = v.VisitDateTime.ToString("HH:mm"),
                    MarkedBy = v.IdMarkedByUserNavigation.Login,
                    MembershipType = v.IdMembershipNavigation?.IdMembershipTypeNavigation.TypeName ?? "—"
                });
            }

            UpdateStatusText();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Ошибка: {ex.Message}";
        }
    }

    private void UpdateStatusText()
    {
        StatusText.Text =
            $"На занятия: {_expectedRows.Count}, в зал (с абонементом): {_gymExpectedRows.Count}, отмечено визитов: {_visitRows.Count}";
    }

    private async Task MarkSelectedAsync(int idClient, string clientName)
    {
        if (!CanMarkToday())
        {
            StatusText.Text = Session.IsReception
                ? "Отмечать можно только за сегодня"
                : "Отмечать визиты может только ресепшн";
            return;
        }

        var error = await VisitService.MarkVisitAsync(_db, idClient, GetSelectedDate());
        if (error != null)
        {
            StatusText.Text = error;
            return;
        }

        StatusText.Text = $"Визит отмечен: {clientName}";
        await LoadDataAsync();
    }

    private async Task OnMarkClassArrivalAsync()
    {
        if (ExpectedGrid.SelectedItem is not ExpectedVisitRow row)
        {
            StatusText.Text = "Выберите клиента из списка записей на занятия";
            return;
        }

        if (!row.CanMark)
        {
            StatusText.Text = row.Status == "Пришёл"
                ? "Визит уже отмечен"
                : "Нельзя отметить эту запись";
            return;
        }

        await MarkSelectedAsync(row.IdClient, row.ClientName);
    }

    private async Task OnMarkGymArrivalAsync()
    {
        if (GymExpectedGrid.SelectedItem is not GymExpectedRow row)
        {
            StatusText.Text = "Выберите клиента из списка посещения зала";
            return;
        }

        if (!row.CanMark)
        {
            StatusText.Text = row.Status == "Пришёл"
                ? "Визит уже отмечен"
                : "Нельзя отметить этого клиента";
            return;
        }

        await MarkSelectedAsync(row.IdClient, row.ClientName);
    }
}
