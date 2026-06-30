using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Fitness_Club_01.Data;
using Fitness_Club_01.Models;
using Fitness_Club_01.Services;
using Fitness_Club_01.Views;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Controls;

public partial class BookingsControl : UserControl
{
    private readonly ApplicationDbContext _db = new();
    private readonly ObservableCollection<BookingRow> _rows = new();
    private readonly bool _clientOnly;
    private readonly bool _trainerOnly;

    public BookingsControl(bool clientOnly = false, bool trainerOnly = false)
    {
        _clientOnly = clientOnly;
        _trainerOnly = trainerOnly;

        InitializeComponent();
        BookingsGrid.ItemsSource = _rows;

        if (_clientOnly)
        {
            HeaderText.Text = "Мои записи";
            AddButton.IsVisible = false;
            DataGridHelper.RemoveColumn(BookingsGrid, 1);
        }

        if (_trainerOnly)
        {
            HeaderText.Text = "Записи на мои занятия";
            AddButton.IsVisible = false;
            CancelBookingButton.IsVisible = false;
            SearchBox.IsVisible = false;
            DataGridHelper.RemoveColumn(BookingsGrid, 5);
        }

        Loaded += (_, _) => LoadData();
        SearchBox.TextChanged += (_, _) => LoadData();
        AddButton.Click += async (_, _) => await OnAddAsync();
        CancelBookingButton.Click += async (_, _) => await OnCancelAsync();
        RefreshButton.Click += (_, _) => LoadData();
        Unloaded += (_, _) => _db.Dispose();
    }

    private Window? GetOwner() => TopLevel.GetTopLevel(this) as Window;

    private void LoadData()
    {
        try
        {
            DbRefresh.ClearCache(_db);
            var search = SearchBox.Text?.Trim().ToLower() ?? "";
            var today = DateOnly.FromDateTime(DateTime.Today);

            var query = _db.ClassBookings
                .Include(b => b.IdClientNavigation)
                .Include(b => b.IdGroupClassNavigation)
                    .ThenInclude(g => g.IdTrainerNavigation)
                .Include(b => b.IdGroupClassNavigation)
                    .ThenInclude(g => g.IdGymHallNavigation)
                .AsNoTracking()
                .AsQueryable();

            if (_clientOnly && Session.CurrentUser?.IdClient != null)
                query = query.Where(b => b.IdClient == Session.CurrentUser.IdClient);

            if (_trainerOnly && Session.CurrentUser?.IdTrainer != null)
                query = query.Where(b => b.IdGroupClassNavigation.IdTrainer == Session.CurrentUser.IdTrainer);

            var list = query
                .OrderByDescending(b => b.ClassDate)
                .Take(500)
                .ToList();

            if (!string.IsNullOrEmpty(search))
            {
                list = list.Where(b =>
                    PersonNameFormatter.FullName(b.IdClientNavigation).ToLower().Contains(search) ||
                    b.IdGroupClassNavigation.ClassName.ToLower().Contains(search)).ToList();
            }

            _rows.Clear();
            var n = 1;
            foreach (var b in list)
            {
                var startTime = b.IdGroupClassNavigation.StartTime;
                _rows.Add(new BookingRow
                {
                    IdClassBooking = b.IdClassBooking,
                    Number = n++,
                    IdClient = b.IdClient,
                    IdGroupClass = b.IdGroupClass,
                    ClientName = PersonNameFormatter.FullName(b.IdClientNavigation),
                    ClassName = b.IdGroupClassNavigation.ClassName,
                    ClassDate = b.ClassDate.ToString("dd.MM.yyyy"),
                    ClassTime = startTime.ToString("HH:mm"),
                    ClassDateTime = $"{b.ClassDate:dd.MM.yyyy} {startTime:HH:mm}",
                    ClassDateValue = b.ClassDate,
                    ClassStartTime = startTime,
                    TrainerName = PersonNameFormatter.FullName(b.IdGroupClassNavigation.IdTrainerNavigation),
                    HallName = b.IdGroupClassNavigation.IdGymHallNavigation.HallName,
                    BookedAt = b.BookedAt.ToString("dd.MM.yyyy HH:mm"),
                    CanCancel = b.ClassDate >= today
                });
            }

            StatusText.Text = $"Показано: {_rows.Count} записей";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Ошибка: {ex.Message}";
        }
    }

    private BookingRow? GetSelected() => BookingsGrid.SelectedItem as BookingRow;

    private async Task OnAddAsync()
    {
        var owner = GetOwner();
        if (owner == null) return;

        var win = new BookingEditWindow(_clientOnly ? Session.CurrentUser?.IdClient : null);
        await win.ShowDialog(owner);
        if (win.Saved) LoadData();
    }

    private async Task OnCancelAsync()
    {
        var row = GetSelected();
        if (row == null)
        {
            StatusText.Text = "Выберите запись";
            return;
        }

        if (!row.CanCancel)
        {
            StatusText.Text = "Нельзя отменить прошедшую запись";
            return;
        }

        if (_clientOnly)
        {
            var leadTimeError = BookingValidator.ValidateCancelLeadTime(row.ClassStartTime, row.ClassDateValue);
            if (leadTimeError != null)
            {
                StatusText.Text = leadTimeError;
                return;
            }
        }

        if (_clientOnly && Session.CurrentUser?.IdClient != row.IdClient)
        {
            StatusText.Text = "Можно отменить только свою запись";
            return;
        }

        var owner = GetOwner();
        if (owner == null) return;

        if (await MessageBox.Show(owner, "Отменить запись на занятие?", "Подтверждение", MessageBoxButtons.YesNo)
            != MessageBoxResult.Yes)
            return;

        var booking = await _db.ClassBookings
            .Include(b => b.IdClientNavigation)
            .Include(b => b.IdGroupClassNavigation)
                .ThenInclude(g => g.IdTrainerNavigation)
            .FirstOrDefaultAsync(b => b.IdClassBooking == row.IdClassBooking);
        if (booking == null) return;

        if (_trainerOnly && Session.CurrentUser?.IdTrainer != booking.IdGroupClassNavigation.IdTrainer)
        {
            StatusText.Text = "Можно отменить только записи на ваши занятия";
            return;
        }

        if (Session.CurrentUser != null && !_trainerOnly)
            await NotificationService.NotifyBookingCancelledAsync(_db, booking, Session.CurrentUser);

        _db.ClassBookings.Remove(booking);
        await _db.SaveChangesAsync();
        LoadData();
    }
}
