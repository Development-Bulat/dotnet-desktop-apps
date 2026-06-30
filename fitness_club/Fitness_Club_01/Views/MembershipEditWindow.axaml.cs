using Avalonia.Controls;
using Avalonia.Interactivity;
using Fitness_Club_01.Data;
using Fitness_Club_01.Services;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Views;

public partial class MembershipEditWindow : Window
{
    private readonly ApplicationDbContext _db = new();
    private readonly int? _fixedClientId;
    private readonly bool _clientPurchase;
    private List<Client> _clients = new();
    private List<MembershipType> _types = new();

    public bool Saved { get; private set; }

    public MembershipEditWindow(int? fixedClientId = null)
    {
        _fixedClientId = fixedClientId;
        _clientPurchase = fixedClientId.HasValue;
        InitializeComponent();

        if (_clientPurchase)
        {
            Title = "Покупка абонемента";
            HeaderTitle.Text = "Покупка абонемента";
            Height = 560;
            ClientCombo.IsVisible = false;
            PaymentPanel.IsVisible = true;
            SaveButton.Content = "Оплатить";
            CardInputFormatter.AttachCardNumber(CardNumberBox);
            CardInputFormatter.AttachExpiry(ExpiryBox);
            CardInputFormatter.AttachCvv(CvvBox);
        }

        TypeCombo.SelectionChanged += (_, _) => UpdateOrderInfo();
        StartDatePicker.SelectedDateChanged += (_, _) => UpdateOrderInfo();

        SaveButton.Click += OnSaveClick;
        CancelButton.Click += (_, _) => Close();
        Loaded += async (_, _) => await LoadAsync();
        Closed += (_, _) => _db.Dispose();
    }

    private async Task LoadAsync()
    {
        _clients = await _db.Clients.OrderBy(c => c.LastName).ToListAsync();
        _types = await _db.MembershipTypes.OrderBy(t => t.TypeName).ToListAsync();

        if (_fixedClientId.HasValue)
        {
            var client = _clients.FirstOrDefault(c => c.IdClient == _fixedClientId.Value);
            if (client == null)
            {
                ErrorText.Text = "Клиент не найден";
                SaveButton.IsEnabled = false;
                return;
            }

            _clients = [client];
        }
        else
        {
            ClientCombo.ItemsSource = _clients.Select(c => new ComboItem(c.IdClient, PersonNameFormatter.FullName(c))).ToList();
        }

        TypeCombo.ItemsSource = _types.Select(t => new ComboItem(t.IdMembershipType, $"{t.TypeName} — {t.Price:N0} ₽")).ToList();

        StartDatePicker.SelectedDate = DateTime.Today;
        UpdateOrderInfo();
    }

    private void UpdateOrderInfo()
    {
        UpdatePeriodText();
        if (_clientPurchase)
            UpdateAmountText();
    }

    private void UpdatePeriodText()
    {
        if (TypeCombo.SelectedItem is not ComboItem typeItem || !StartDatePicker.SelectedDate.HasValue)
        {
            PeriodText.Text = "Период: выберите тариф и дату начала";
            return;
        }

        var type = _types.FirstOrDefault(t => t.IdMembershipType == typeItem.Id);
        if (type == null)
        {
            PeriodText.Text = "";
            return;
        }

        var start = DateOnly.FromDateTime(StartDatePicker.SelectedDate!.Value.DateTime);
        var end = start.AddDays(type.DurationDays - 1);
        PeriodText.Text = $"Период абонемента: {start:dd.MM.yyyy} — {end:dd.MM.yyyy}";
    }

    private void UpdateAmountText()
    {
        if (!_clientPurchase)
            return;

        if (TypeCombo.SelectedItem is ComboItem typeItem)
        {
            var type = _types.FirstOrDefault(t => t.IdMembershipType == typeItem.Id);
            AmountText.Text = type == null
                ? "Выберите тариф"
                : $"К оплате: {type.Price:N0} ₽";
        }
        else
        {
            AmountText.Text = "Выберите тариф";
        }
    }

    private async void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        ErrorText.Text = "";

        if (!TryGetOrderData(out var clientId, out var type, out var start))
            return;

        var activeStatus = await _db.MembershipStatuses.FirstAsync(s => s.StatusCode == "active");
        var today = DateOnly.FromDateTime(DateTime.Today);

        var hasActive = await _db.Memberships.AnyAsync(m =>
            m.IdClient == clientId
            && m.IdMembershipStatus == activeStatus.IdMembershipStatus
            && m.EndDate >= today);

        if (hasActive)
        {
            var pending = await _db.Memberships
                .Where(m => m.IdClient == clientId
                            && m.IdMembershipStatus == activeStatus.IdMembershipStatus
                            && m.StartDate > today)
                .OrderBy(m => m.StartDate)
                .FirstOrDefaultAsync();

            ErrorText.Text = pending != null
                ? (_clientPurchase
                    ? $"У вас уже оформлен абонемент с {pending.StartDate:dd.MM.yyyy}"
                    : $"У клиента уже оформлен абонемент с {pending.StartDate:dd.MM.yyyy}")
                : (_clientPurchase
                    ? "У вас уже есть активный абонемент"
                    : "У клиента уже есть активный абонемент");
            return;
        }

        if (_clientPurchase)
        {
            var paymentError = ValidatePayment();
            if (paymentError != null)
            {
                ErrorText.Text = paymentError;
                return;
            }

            await MessageBox.Show(
                this,
                $"Оплата прошла успешно.\nСумма: {type.Price:N0} ₽\nАбонемент «{type.TypeName}»\n{start:dd.MM.yyyy} — {start.AddDays(type.DurationDays - 1):dd.MM.yyyy}",
                "Успешно оплачено",
                MessageBoxButtons.Ok);
        }

        await SaveMembershipAsync(clientId, type, start, activeStatus.IdMembershipStatus);
        Saved = true;
        Close();
    }

    private bool TryGetOrderData(out int clientId, out MembershipType type, out DateOnly start)
    {
        clientId = 0;
        type = null!;
        start = default;

        if (_fixedClientId.HasValue)
        {
            clientId = _fixedClientId.Value;
        }
        else if (ClientCombo.SelectedItem is ComboItem clientItem)
        {
            clientId = clientItem.Id;
        }
        else
        {
            ErrorText.Text = "Выберите клиента и тариф";
            return false;
        }

        if (TypeCombo.SelectedItem is not ComboItem typeItem)
        {
            ErrorText.Text = "Выберите тариф";
            return false;
        }

        if (!StartDatePicker.SelectedDate.HasValue)
        {
            ErrorText.Text = "Укажите дату начала";
            return false;
        }

        var membershipType = _types.FirstOrDefault(t => t.IdMembershipType == typeItem.Id);
        if (membershipType == null)
        {
            ErrorText.Text = "Тариф не найден";
            return false;
        }

        start = DateOnly.FromDateTime(StartDatePicker.SelectedDate!.Value.DateTime);

        var startError = DateValidator.ValidateMembershipStart(start);
        if (startError != null)
        {
            ErrorText.Text = startError;
            return false;
        }

        type = membershipType;
        return true;
    }

    private string? ValidatePayment()
    {
        return CardPaymentValidator.ValidateCardNumber(CardInputFormatter.GetCardDigits(CardNumberBox))
               ?? CardPaymentValidator.ValidateExpiry(CardInputFormatter.GetExpiryDigits(ExpiryBox))
               ?? CardPaymentValidator.ValidateCvv(CvvBox.Text?.Trim() ?? "")
               ?? CardPaymentValidator.ValidateHolder(HolderBox.Text?.Trim() ?? "");
    }

    private async Task SaveMembershipAsync(int clientId, MembershipType type, DateOnly start, int activeStatusId)
    {
        var end = start.AddDays(type.DurationDays - 1);

        var membership = new Membership
        {
            IdClient = clientId,
            IdMembershipType = type.IdMembershipType,
            StartDate = start,
            EndDate = end,
            IdMembershipStatus = activeStatusId,
            SoldAt = DateTimeDb.Now,
            IdSoldByUser = Session.CurrentUser?.IdUserAccount
        };

        _db.Memberships.Add(membership);
        await _db.SaveChangesAsync();

        _db.MembershipStatusHistories.Add(new MembershipStatusHistory
        {
            IdMembership = membership.IdMembership,
            IdMembershipStatus = activeStatusId,
            ChangedAt = DateTimeDb.Now,
            IdChangedByUser = Session.CurrentUser?.IdUserAccount,
            Comment = _clientPurchase ? "Покупка абонемента клиентом (имитация оплаты)" : "Оформление абонемента"
        });

        await _db.SaveChangesAsync();
    }

    private sealed record ComboItem(int Id, string Title)
    {
        public override string ToString() => Title;
    }
}
