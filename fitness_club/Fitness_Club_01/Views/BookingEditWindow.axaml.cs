using Avalonia.Controls;
using Avalonia.Interactivity;
using Fitness_Club_01.Data;
using Fitness_Club_01.Services;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Views;

public partial class BookingEditWindow : Window
{
    private readonly ApplicationDbContext _db = new();
    private readonly int? _fixedClientId;
    private List<GroupClass> _classes = new();

    public bool Saved { get; private set; }

    public BookingEditWindow(int? fixedClientId = null)
    {
        _fixedClientId = fixedClientId;
        InitializeComponent();
        if (_fixedClientId.HasValue)
            SaveButton.Content = "Записаться";

        SaveButton.Click += OnSaveClick;
        CancelButton.Click += (_, _) => Close();
        Loaded += async (_, _) => await LoadAsync();
        Closed += (_, _) => _db.Dispose();
    }

    private async Task LoadAsync()
    {
        _classes = await _db.GroupClasses
            .Include(g => g.IdDayOfWeekNavigation)
            .Include(g => g.IdTrainerNavigation)
            .Include(g => g.IdGymHallNavigation)
            .Where(g => g.IsActive)
            .OrderBy(g => g.ClassName)
            .ToListAsync();

        ClassCombo.ItemsSource = _classes.Select(g =>
            new ComboItem(g.IdGroupClass,
                $"{g.ClassName} ({g.IdDayOfWeekNavigation.DayName} {g.StartTime:HH\\:mm})")).ToList();

        ClassDatePicker.SelectedDate = DateTime.Today.AddDays(1);

        if (_fixedClientId.HasValue)
        {
            var client = await _db.Clients.FindAsync(_fixedClientId.Value);
            if (client != null)
            {
                ClientCombo.ItemsSource = new[] { new ComboItem(client.IdClient, PersonNameFormatter.FullName(client)) };
                ClientCombo.SelectedIndex = 0;
                ClientCombo.IsEnabled = false;
            }
        }
        else
        {
            var clients = await _db.Clients.OrderBy(c => c.LastName).ToListAsync();
            ClientCombo.ItemsSource = clients.Select(c => new ComboItem(c.IdClient, PersonNameFormatter.FullName(c))).ToList();
        }
    }

    private async void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        if (Session.CurrentUser == null)
        {
            ErrorText.Text = "Сессия недействительна";
            return;
        }

        if (ClientCombo.SelectedItem is not ComboItem clientItem ||
            ClassCombo.SelectedItem is not ComboItem classItem)
        {
            ErrorText.Text = "Выберите клиента и занятие";
            return;
        }

        if (!ClassDatePicker.SelectedDate.HasValue)
        {
            ErrorText.Text = "Укажите дату занятия";
            return;
        }

        var classDate = DateOnly.FromDateTime(ClassDatePicker.SelectedDate!.Value.DateTime);

        var dateError = DateValidator.ValidateBookingDate(classDate);
        if (dateError != null)
        {
            ErrorText.Text = dateError;
            return;
        }

        var membershipError = await MembershipService.ValidateMembershipForDateAsync(
            _db,
            clientItem.Id,
            classDate,
            _fixedClientId.HasValue);

        if (membershipError != null)
        {
            ErrorText.Text = membershipError;
            return;
        }

        var groupClass = _classes.First(g => g.IdGroupClass == classItem.Id);

        var bookingError = await ScheduleValidator.ValidateBookingAsync(_db, groupClass, classDate);
        if (bookingError != null)
        {
            ErrorText.Text = bookingError;
            return;
        }

        var duplicate = await _db.ClassBookings.AnyAsync(b =>
            b.IdClient == clientItem.Id
            && b.IdGroupClass == classItem.Id
            && b.ClassDate == classDate);

        if (duplicate)
        {
            ErrorText.Text = "Клиент уже записан на это занятие";
            return;
        }

        try
        {
            _db.ClassBookings.Add(new ClassBooking
            {
                IdClient = clientItem.Id,
                IdGroupClass = classItem.Id,
                ClassDate = classDate,
                BookedAt = DateTimeDb.Now,
                IdBookedByUser = Session.CurrentUser.IdUserAccount
            });

            await _db.SaveChangesAsync();
            Saved = true;
            Close();
        }
        catch (Exception ex)
        {
            ErrorText.Text = ex.InnerException?.Message ?? ex.Message;
        }
    }

    private sealed record ComboItem(int Id, string Title)
    {
        public override string ToString() => Title;
    }
}
