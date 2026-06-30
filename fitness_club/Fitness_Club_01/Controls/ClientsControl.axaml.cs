using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Fitness_Club_01.Data;
using Fitness_Club_01.Models;
using Fitness_Club_01.Services;
using Fitness_Club_01.Views;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Controls;

public partial class ClientsControl : UserControl
{
    private readonly ApplicationDbContext _db = new();
    private readonly ObservableCollection<ClientRow> _rows = new();

    public ClientsControl()
    {
        InitializeComponent();
        ClientsGrid.ItemsSource = _rows;

        if (Session.IsReception)
        {
            EditButton.IsVisible = false;
            DeleteButton.IsVisible = false;
        }
        else if (Session.IsAdmin)
        {
            AddButton.IsVisible = false;
            EditButton.IsVisible = false;
            AccountButton.IsVisible = false;
        }

        Loaded += (_, _) => LoadData();
        SearchBox.TextChanged += (_, _) => LoadData();
        AddButton.Click += async (_, _) => await OnAddAsync();
        EditButton.Click += async (_, _) => await OnEditAsync();
        AccountButton.Click += async (_, _) => await OnAccountAsync();
        RefreshButton.Click += (_, _) => LoadData();
        DeleteButton.Click += async (_, _) => await OnDeleteAsync();
        Unloaded += (_, _) => _db.Dispose();
    }

    private Window? GetOwner() => TopLevel.GetTopLevel(this) as Window;

    private void LoadData()
    {
        try
        {
            DbRefresh.ClearCache(_db);
            var search = SearchBox.Text?.Trim().ToLower() ?? "";

            var clients = _db.Clients
                .Include(c => c.UserAccounts)
                .AsNoTracking()
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToList();

            if (!string.IsNullOrEmpty(search))
            {
                var searchDigits = PhoneInputFormatter.ExtractNationalDigits(search);
                clients = clients.Where(c =>
                    PersonNameFormatter.FullName(c).ToLower().Contains(search) ||
                    c.Phone.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (searchDigits.Length > 0 &&
                     PhoneInputFormatter.ExtractNationalDigits(c.Phone).Contains(searchDigits, StringComparison.Ordinal))).ToList();
            }

            _rows.Clear();
            var n = 1;
            foreach (var c in clients)
            {
                var account = c.UserAccounts.FirstOrDefault();
                _rows.Add(new ClientRow
                {
                    IdClient = c.IdClient,
                    Number = n++,
                    LastName = c.LastName,
                    FirstName = c.FirstName,
                    Patronymic = c.Patronymic,
                    FullName = PersonNameFormatter.FullName(c),
                    Phone = PhoneInputFormatter.FormatStored(c.Phone),
                    BirthDateText = c.BirthDate?.ToString("dd.MM.yyyy") ?? "—",
                    HasAccount = account != null ? "Да" : "Нет",
                    Login = account?.Login ?? "—"
                });
            }

            StatusText.Text = $"Показано: {_rows.Count} клиентов";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Ошибка: {ex.Message}";
        }
    }

    private ClientRow? GetSelected() =>
        ClientsGrid.SelectedItem as ClientRow;

    private async Task OnAddAsync()
    {
        if (Session.IsAdmin)
        {
            StatusText.Text = "Администратор не может добавлять клиентов";
            return;
        }

        var owner = GetOwner();
        if (owner == null) return;

        var win = new ClientEditWindow();
        await win.ShowDialog(owner);
        if (win.Saved) LoadData();
    }

    private async Task OnEditAsync()
    {
        if (Session.IsAdmin)
        {
            StatusText.Text = "Администратор не может изменять данные клиента";
            return;
        }

        if (Session.IsReception)
        {
            StatusText.Text = "Ресепшн не может изменять данные клиента";
            return;
        }

        var row = GetSelected();
        if (row == null)
        {
            StatusText.Text = "Выберите клиента";
            return;
        }

        var owner = GetOwner();
        if (owner == null) return;

        var win = new ClientEditWindow(row.IdClient);
        await win.ShowDialog(owner);
        if (win.Saved) LoadData();
    }

    private async Task OnAccountAsync()
    {
        if (Session.IsAdmin)
        {
            StatusText.Text = "Администратор не может выдавать логины клиентам";
            return;
        }

        var row = GetSelected();
        if (row == null)
        {
            StatusText.Text = "Выберите клиента";
            return;
        }

        if (row.HasAccount == "Да")
        {
            StatusText.Text = "У клиента уже есть учётная запись";
            return;
        }

        var owner = GetOwner();
        if (owner == null) return;

        var win = new ClientAccountWindow(row.IdClient);
        await win.ShowDialog(owner);
        if (win.Saved) LoadData();
    }

    private async Task OnDeleteAsync()
    {
        if (Session.IsReception)
        {
            StatusText.Text = "Ресепшн не может удалять клиентов";
            return;
        }

        var row = GetSelected();
        if (row == null)
        {
            StatusText.Text = "Выберите клиента";
            return;
        }

        var owner = GetOwner();
        if (owner == null) return;

        if (await MessageBox.Show(owner,
                $"Удалить клиента {row.FullName}?",
                "Подтверждение",
                MessageBoxButtons.YesNo) != MessageBoxResult.Yes)
            return;

        try
        {
            var client = await _db.Clients
                .Include(c => c.UserAccounts)
                .Include(c => c.Memberships)
                .Include(c => c.ClassBookings)
                .Include(c => c.Visits)
                .FirstOrDefaultAsync(c => c.IdClient == row.IdClient);

            if (client == null) return;

            if (client.Memberships.Count > 0 || client.ClassBookings.Count > 0 || client.Visits.Count > 0)
            {
                StatusText.Text = "Нельзя удалить: есть абонементы, записи или визиты";
                return;
            }

            foreach (var account in client.UserAccounts.ToList())
                _db.UserAccounts.Remove(account);

            _db.Clients.Remove(client);
            await _db.SaveChangesAsync();
            LoadData();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Ошибка: {ex.Message}";
        }
    }
}
