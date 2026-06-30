using Fitness_Club_01.Data;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Services;

public static class NotificationService
{
    public static event Action? UnreadCountChanged;

    public static async Task<int> GetUnreadCountAsync(ApplicationDbContext db, int userAccountId)
    {
        return await db.Notifications
            .AsNoTracking()
            .CountAsync(n => n.IdUserAccount == userAccountId && !n.IsRead);
    }

    public static async Task NotifyBookingCancelledAsync(
        ApplicationDbContext db,
        ClassBooking booking,
        UserAccount cancelledBy)
    {
        var groupClass = booking.IdGroupClassNavigation;
        var client = booking.IdClientNavigation;
        var trainer = groupClass.IdTrainerNavigation;
        var className = groupClass.ClassName;
        var dateText = booking.ClassDate.ToString("dd.MM.yyyy");
        var timeText = groupClass.StartTime.ToString("HH:mm");
        var clientName = PersonNameFormatter.FullName(client);
        var trainerName = PersonNameFormatter.FullName(trainer);

        var cancelledByClient = cancelledBy.IdClient == booking.IdClient;
        var cancelledByTrainer = cancelledBy.IdTrainer == groupClass.IdTrainer;

        if (cancelledByClient)
        {
            await CreateForTrainerAsync(
                db,
                groupClass.IdTrainer,
                "Отмена записи клиентом",
                $"Клиент {clientName} отменил запись на «{className}» {dateText} в {timeText}.");
        }
        else if (cancelledByTrainer)
        {
            await CreateForClientAsync(
                db,
                booking.IdClient,
                "Отмена записи тренером",
                $"Тренер {trainerName} отменил вашу запись на «{className}» {dateText} в {timeText}.");
        }
        else
        {
            await CreateForClientAsync(
                db,
                booking.IdClient,
                "Отмена записи",
                $"Сотрудник клуба отменил вашу запись на «{className}» {dateText} в {timeText}.");
            await CreateForTrainerAsync(
                db,
                groupClass.IdTrainer,
                "Отмена записи",
                $"Сотрудник клуба отменил запись клиента {clientName} на «{className}» {dateText} в {timeText}.");
        }

        UnreadCountChanged?.Invoke();
    }

    public static async Task NotifyClassSessionCancelledAsync(
        ApplicationDbContext db,
        GroupClass groupClass,
        DateOnly classDate,
        IReadOnlyList<ClassBooking> bookings)
    {
        var trainerName = PersonNameFormatter.FullName(groupClass.IdTrainerNavigation);
        var className = groupClass.ClassName;
        var dateText = classDate.ToString("dd.MM.yyyy");
        var timeText = groupClass.StartTime.ToString("HH:mm");
        var title = "Занятие отменено";
        var message =
            $"Тренер {trainerName} отменил занятие «{className}» {dateText} в {timeText}. Ваша запись удалена.";

        var clientIds = bookings
            .Select(b => b.IdClient)
            .Distinct()
            .ToList();

        foreach (var clientId in clientIds)
            await CreateForClientAsync(db, clientId, title, message);

        UnreadCountChanged?.Invoke();
    }

    public static async Task MarkAllReadAsync(ApplicationDbContext db, int userAccountId)
    {
        var unread = await db.Notifications
            .Where(n => n.IdUserAccount == userAccountId && !n.IsRead)
            .ToListAsync();

        if (unread.Count == 0)
            return;

        foreach (var item in unread)
            item.IsRead = true;

        await db.SaveChangesAsync();
        UnreadCountChanged?.Invoke();
    }

    private static async Task CreateForClientAsync(ApplicationDbContext db, int clientId, string title, string message)
    {
        var userIds = await db.UserAccounts
            .AsNoTracking()
            .Where(u => u.IdClient == clientId)
            .Select(u => u.IdUserAccount)
            .ToListAsync();

        await CreateManyAsync(db, userIds, title, message);
    }

    private static async Task CreateForTrainerAsync(ApplicationDbContext db, int trainerId, string title, string message)
    {
        var userIds = await db.UserAccounts
            .AsNoTracking()
            .Where(u => u.IdTrainer == trainerId)
            .Select(u => u.IdUserAccount)
            .ToListAsync();

        await CreateManyAsync(db, userIds, title, message);
    }

    private static async Task CreateManyAsync(ApplicationDbContext db, IReadOnlyList<int> userIds, string title, string message)
    {
        if (userIds.Count == 0)
            return;

        foreach (var userId in userIds)
        {
            db.Notifications.Add(new Notification
            {
                IdUserAccount = userId,
                Title = title,
                Message = message,
                CreatedAt = DateTimeDb.Now,
                IsRead = false
            });
        }

        await db.SaveChangesAsync();
    }
}
