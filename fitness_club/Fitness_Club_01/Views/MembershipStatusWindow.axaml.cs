using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Fitness_Club_01.Data;
using Fitness_Club_01.Models;
using Fitness_Club_01.Services;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Views;

public partial class MembershipStatusWindow : Window
{
    private readonly ApplicationDbContext _db = new();
    private readonly int _idMembership;
    private readonly ObservableCollection<MembershipHistoryRow> _history = new();
    private string _statusCode = "";

    public bool Changed { get; private set; }

    public MembershipStatusWindow(int idMembership)
    {
        _idMembership = idMembership;
        InitializeComponent();
        HistoryGrid.ItemsSource = _history;
        FreezeButton.Click += async (_, _) => await ChangeStatusAsync("frozen", "Заморозка абонемента");
        UnfreezeButton.Click += async (_, _) => await ChangeStatusAsync("active", "Разморозка абонемента");
        CancelButton.Click += async (_, _) => await ChangeStatusAsync("cancelled", "Отмена абонемента");
        CloseButton.Click += (_, _) => Close();
        Loaded += async (_, _) => await LoadAsync();
        Closed += (_, _) => _db.Dispose();
    }

    private async Task LoadAsync()
    {
        await MembershipService.ExpireOutdatedAsync(_db);

        var membership = await _db.Memberships
            .Include(m => m.IdClientNavigation)
            .Include(m => m.IdMembershipTypeNavigation)
            .Include(m => m.IdMembershipStatusNavigation)
            .FirstOrDefaultAsync(m => m.IdMembership == _idMembership);

        if (membership == null)
        {
            ErrorText.Text = "Абонемент не найден";
            return;
        }

        _statusCode = membership.IdMembershipStatusNavigation.StatusCode;
        TitleText.Text = PersonNameFormatter.FullName(membership.IdClientNavigation);
        InfoText.Text =
            $"{membership.IdMembershipTypeNavigation.TypeName} | {membership.StartDate:dd.MM.yyyy} — {membership.EndDate:dd.MM.yyyy} | Статус: {membership.IdMembershipStatusNavigation.StatusName}";

        FreezeButton.IsEnabled = _statusCode == "active";
        UnfreezeButton.IsEnabled = _statusCode == "frozen";
        CancelButton.IsEnabled = _statusCode is "active" or "frozen";

        var history = await _db.MembershipStatusHistories
            .Include(h => h.IdMembershipStatusNavigation)
            .Include(h => h.IdChangedByUserNavigation)
            .Where(h => h.IdMembership == _idMembership)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();

        _history.Clear();
        var n = 1;
        foreach (var h in history)
        {
            _history.Add(new MembershipHistoryRow
            {
                Number = n++,
                ChangedAt = h.ChangedAt.ToString("dd.MM.yyyy HH:mm"),
                StatusName = h.IdMembershipStatusNavigation.StatusName,
                ChangedBy = h.IdChangedByUserNavigation?.Login ?? "—",
                Comment = h.Comment ?? ""
            });
        }
    }

    private async Task ChangeStatusAsync(string statusCode, string comment)
    {
        ErrorText.Text = "";
        var error = await MembershipService.ChangeStatusAsync(
            _db,
            _idMembership,
            statusCode,
            comment,
            Session.CurrentUser?.IdUserAccount);

        if (error != null)
        {
            ErrorText.Text = error;
            return;
        }

        Changed = true;
        await LoadAsync();
    }
}
