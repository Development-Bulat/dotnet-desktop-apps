using Avalonia.Controls;
using Avalonia.Interactivity;
using Fitness_Club_01.Data;
using Fitness_Club_01.Services;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Views;

public partial class VisitMarkWindow : Window
{
    private readonly ApplicationDbContext _db = new();

    public bool Saved { get; private set; }

    public VisitMarkWindow()
    {
        InitializeComponent();
        SaveButton.Click += OnSaveClick;
        CancelButton.Click += (_, _) => Close();
        Loaded += async (_, _) => await LoadAsync();
        Closed += (_, _) => _db.Dispose();
    }

    private async Task LoadAsync()
    {
        if (!Session.IsReception)
        {
            ErrorText.Text = "Отмечать визиты может только сотрудник ресепшн";
            SaveButton.IsEnabled = false;
            return;
        }

        var clients = await _db.Clients.OrderBy(c => c.LastName).ToListAsync();
        ClientCombo.ItemsSource = clients.Select(c => new ComboItem(c.IdClient, PersonNameFormatter.FullName(c))).ToList();

        var (current, capacity) = await VisitService.GetClubOccupancyAsync(_db);
        if (capacity > 0)
            OccupancyHintText.Text = $"Сейчас в клубе: {current}/{capacity}";
    }

    private async void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        if (ClientCombo.SelectedItem is not ComboItem clientItem)
        {
            ErrorText.Text = "Выберите клиента";
            return;
        }

        var error = await VisitService.MarkVisitAsync(
            _db,
            clientItem.Id,
            DateOnly.FromDateTime(DateTime.Today));

        if (error != null)
        {
            ErrorText.Text = error;
            return;
        }

        Saved = true;
        Close();
    }

    private sealed record ComboItem(int Id, string Title)
    {
        public override string ToString() => Title;
    }
}
