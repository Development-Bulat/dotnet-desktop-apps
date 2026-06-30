using Fitness_Club_01.Data;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Services;

public static class ReportService
{
    public static async Task<string> BuildVisitsReportAsync(ApplicationDbContext db, DateOnly from, DateOnly to)
    {
        await MembershipService.ExpireOutdatedAsync(db);

        var visits = await db.Visits
            .Include(v => v.IdClientNavigation)
            .Include(v => v.IdMembershipNavigation)
                .ThenInclude(m => m!.IdMembershipTypeNavigation)
            .Where(v => DateOnly.FromDateTime(v.VisitDateTime) >= from
                        && DateOnly.FromDateTime(v.VisitDateTime) <= to)
            .OrderBy(v => v.VisitDateTime)
            .ToListAsync();

        var lines = new List<string>
        {
            $"ОТЧЁТ ПО ВИЗИТАМ",
            $"Период: {from:dd.MM.yyyy} — {to:dd.MM.yyyy}",
            $"Всего визитов: {visits.Count}",
            "",
            "Дата и время          | Клиент                          | Тариф",
            new string('-', 78)
        };

        foreach (var v in visits)
        {
            var name = PersonNameFormatter.FullName(v.IdClientNavigation);
            var type = v.IdMembershipNavigation?.IdMembershipTypeNavigation.TypeName ?? "—";
            lines.Add($"{v.VisitDateTime:dd.MM.yyyy HH:mm}  | {name,-30} | {type}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    public static async Task<string> BuildSalesReportAsync(ApplicationDbContext db, DateOnly from, DateOnly to)
    {
        await MembershipService.ExpireOutdatedAsync(db);

        var memberships = await db.Memberships
            .Include(m => m.IdClientNavigation)
            .Include(m => m.IdMembershipTypeNavigation)
            .Include(m => m.IdMembershipStatusNavigation)
            .Where(m => DateOnly.FromDateTime(m.SoldAt) >= from
                        && DateOnly.FromDateTime(m.SoldAt) <= to)
            .OrderBy(m => m.SoldAt)
            .ToListAsync();

        var total = memberships.Sum(m => m.IdMembershipTypeNavigation.Price);

        var lines = new List<string>
        {
            "ОТЧЁТ ПО ПРОДАЖАМ АБОНЕМЕНТОВ",
            $"Период: {from:dd.MM.yyyy} — {to:dd.MM.yyyy}",
            $"Продано: {memberships.Count} шт.",
            $"Сумма: {total:N0} ₽",
            "",
            "Дата продажи | Клиент                          | Тариф               | Сумма    | Статус",
            new string('-', 95)
        };

        foreach (var m in memberships)
        {
            lines.Add(
                $"{m.SoldAt:dd.MM.yyyy}   | {PersonNameFormatter.FullName(m.IdClientNavigation),-30} | {m.IdMembershipTypeNavigation.TypeName,-18} | {m.IdMembershipTypeNavigation.Price,7:N0} ₽ | {m.IdMembershipStatusNavigation.StatusName}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    public static async Task<string> BuildExpiringReportAsync(ApplicationDbContext db, int withinDays = 14)
    {
        await MembershipService.ExpireOutdatedAsync(db);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var until = today.AddDays(withinDays);
        var active = await db.MembershipStatuses.FirstAsync(s => s.StatusCode == "active");

        var list = await db.Memberships
            .Include(m => m.IdClientNavigation)
            .Include(m => m.IdMembershipTypeNavigation)
            .Where(m => m.IdMembershipStatus == active.IdMembershipStatus
                        && m.EndDate >= today
                        && m.EndDate <= until)
            .OrderBy(m => m.EndDate)
            .ToListAsync();

        var lines = new List<string>
        {
            $"АБОНЕМЕНТЫ, ИСТЕКАЮЩИЕ В БЛИЖАЙШИЕ {withinDays} ДН.",
            $"Найдено: {list.Count}",
            "",
            "Окончание  | Клиент                          | Тариф",
            new string('-', 70)
        };

        foreach (var m in list)
        {
            lines.Add($"{m.EndDate:dd.MM.yyyy}  | {PersonNameFormatter.FullName(m.IdClientNavigation),-30} | {m.IdMembershipTypeNavigation.TypeName}");
        }

        return string.Join(Environment.NewLine, lines);
    }
}
