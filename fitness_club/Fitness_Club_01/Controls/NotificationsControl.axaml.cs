using System.Collections.ObjectModel;
using Avalonia.Controls;
using Fitness_Club_01.Data;
using Fitness_Club_01.Models;
using Fitness_Club_01.Services;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Controls;

public partial class NotificationsControl : UserControl
{
    private readonly ApplicationDbContext _db = new();
    private readonly ObservableCollection<NotificationRow> _rows = new();

    public NotificationsControl()
    {
        InitializeComponent();
        NotificationsGrid.ItemsSource = _rows;

        Loaded += async (_, _) => await LoadDataAsync();
        RefreshButton.Click += async (_, _) => await LoadDataAsync();
        Unloaded += (_, _) => _db.Dispose();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            if (Session.CurrentUser == null)
            {
                StatusText.Text = "Пользователь не авторизован";
                return;
            }

            DbRefresh.ClearCache(_db);
            var userId = Session.CurrentUser.IdUserAccount;

            var list = await _db.Notifications
                .AsNoTracking()
                .Where(n => n.IdUserAccount == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(200)
                .ToListAsync();

            _rows.Clear();
            var n = 1;
            var unread = 0;
            foreach (var item in list)
            {
                if (!item.IsRead)
                    unread++;

                _rows.Add(new NotificationRow
                {
                    IdNotification = item.IdNotification,
                    Number = n++,
                    Title = item.Title,
                    Message = item.Message,
                    CreatedAt = DateTimeDb.FormatLocal(item.CreatedAt),
                    Status = item.IsRead ? "Прочитано" : "Новое",
                    IsUnread = !item.IsRead
                });
            }

            SummaryText.Text = unread > 0
                ? $"Непрочитанных: {unread}"
                : "Нет новых уведомлений";

            if (unread > 0)
                await NotificationService.MarkAllReadAsync(_db, userId);

            StatusText.Text = $"Показано: {_rows.Count} уведомлений";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Ошибка: {ex.Message}";
        }
    }
}
