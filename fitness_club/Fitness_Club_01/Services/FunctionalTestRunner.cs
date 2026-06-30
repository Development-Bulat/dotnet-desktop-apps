using Fitness_Club_01.Data;
using Fitness_Club_01.Services;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Services;

public static class FunctionalTestRunner
{
    public static async Task<int> RunAsync()
    {
        var failed = 0;
        void Pass(string name) => Console.WriteLine($"  OK  {name}");
        void Fail(string name, string detail)
        {
            failed++;
            Console.WriteLine($"  FAIL {name}: {detail}");
        }

        Console.WriteLine("=== Fitness Club — функциональные тесты ===\n");

        await using var db = new ApplicationDbContext();

        try
        {
            await db.Database.CanConnectAsync();
            Pass("Подключение к PostgreSQL");
        }
        catch (Exception ex)
        {
            Fail("Подключение к PostgreSQL", ex.Message);
            return 1;
        }

        var demos = new (string Login, string Password)[]
        {
            ("admin_fc", "Admin_fc1!"),
            ("reception_fc", "Reception1!"),
            ("trainer_volkova", "Trainer1!v"),
            ("client_ivanov", "Client1!iv")
        };

        foreach (var (login, password) in demos)
        {
            var user = await db.UserAccounts.FirstOrDefaultAsync(u => u.Login == login);
            if (user == null)
            {
                Fail($"Учётка {login}", "не найдена");
                continue;
            }

            if (user.PasswordHash != password)
            {
                Fail($"Пароль {login}", "не совпадает с демо");
                continue;
            }

            if (LoginValidator.Validate(login) != null)
                Fail($"Логин {login}", "не проходит валидацию");
            else if (PasswordValidator.Validate(password) != null)
                Fail($"Пароль {login}", "не проходит валидацию");
            else
                Pass($"Вход {login}");
        }

        if (await db.Clients.CountAsync() < 3)
            Fail("Клиенты", "слишком мало записей");
        else
            Pass($"Клиенты ({await db.Clients.CountAsync()} шт.)");

        try
        {
            await MembershipService.ExpireOutdatedAsync(db);
            Pass("Автоистечение абонементов");
        }
        catch (Exception ex)
        {
            Fail("Автоистечение абонементов", ex.Message);
        }

        var ivanov = await db.Clients.FirstOrDefaultAsync(c => c.LastName == "Иванов");
        if (ivanov == null)
            Fail("Клиент Иванов", "не найден");
        else
        {
            var membership = await MembershipService.GetActiveMembershipAsync(db, ivanov.IdClient);
            if (membership == null)
                Fail("Абонемент Иванова", "нет активного");
            else
                Pass("Активный абонемент Иванова");
        }

        var yogaClass = await db.GroupClasses
            .Include(g => g.IdDayOfWeekNavigation)
            .FirstOrDefaultAsync(g => g.ClassName.Contains("Йога"));

        if (yogaClass == null)
            Fail("Занятие йоги", "не найдено");
        else
        {
            var dayNum = yogaClass.IdDayOfWeekNavigation.DayNumber;
            var testDate = NextDateWithDayNumber(dayNum);
            var err = BookingValidator.ValidateClassDate(yogaClass, testDate);
            if (err != null)
                Fail("Валидация дня занятия", err);
            else
                Pass("Валидация дня занятия");

            var wrongDate = testDate.AddDays(1);
            if (BookingValidator.ValidateClassDate(yogaClass, wrongDate) == null)
                Fail("Валидация неверного дня", "должна отклонять");
            else
                Pass("Отклонение неверного дня");
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        try
        {
            var visits = await ReportService.BuildVisitsReportAsync(db, today.AddDays(-30), today);
            if (visits.Length < 50) Fail("Отчёт визитов", "пустой");
            else Pass("Отчёт визитов");

            var sales = await ReportService.BuildSalesReportAsync(db, today.AddDays(-90), today);
            if (sales.Length < 50) Fail("Отчёт продаж", "пустой");
            else Pass("Отчёт продаж");

            var expiring = await ReportService.BuildExpiringReportAsync(db);
            if (expiring.Length < 30) Fail("Отчёт истекающих", "пустой");
            else Pass("Отчёт истекающих");
        }
        catch (Exception ex)
        {
            Fail("Отчёты", ex.Message);
        }

        if (BirthDateValidator.Validate(today.AddYears(-17)) == null)
            Fail("Возраст 17 лет", "должен отклоняться");
        else
            Pass("Отклонение возраста < 18");

        if (BirthDateValidator.Validate(today.AddYears(-25)) != null)
            Fail("Возраст 25 лет", "должен приниматься");
        else
            Pass("Принятие возраста 25+");

        if (PhoneValidator.ValidateAndNormalize("+7 (900) 123-45-67", out var phone) != null)
            Fail("Телефон", "маска не прошла");
        else if (phone != "+79001234567")
            Fail("Телефон", $"нормализация: {phone}");
        else
            Pass("Валидация телефона");

        var under18 = await db.Clients.CountAsync(c =>
            c.BirthDate > today.AddYears(-18));
        if (under18 > 0)
            Fail("Данные БД", $"{under18} клиентов младше 18");
        else
            Pass("Все клиенты 18+ в БД");

        if (ScheduleValidator.TimeRangesOverlap(new TimeOnly(10, 0), 60, new TimeOnly(10, 30), 60))
            Pass("Пересечение расписания: слоты накладываются");
        else
            Fail("Пересечение расписания", "не обнаружено пересечение 10:00 и 10:30");

        if (!ScheduleValidator.TimeRangesOverlap(new TimeOnly(10, 0), 60, new TimeOnly(11, 0), 60))
            Pass("Пересечение расписания: подряд идущие слоты");
        else
            Fail("Пересечение расписания", "ложное пересечение 10:00 и 11:00");

        var classes = await db.GroupClasses
            .Include(g => g.IdDayOfWeekNavigation)
            .Where(g => g.IsActive)
            .ToListAsync();

        var overlapPairs = 0;
        for (var i = 0; i < classes.Count; i++)
        {
            for (var j = i + 1; j < classes.Count; j++)
            {
                var a = classes[i];
                var b = classes[j];
                if (a.IdGymHall != b.IdGymHall || a.IdDayOfWeek != b.IdDayOfWeek)
                    continue;

                if (ScheduleValidator.TimeRangesOverlap(a.StartTime, a.DurationMinutes, b.StartTime, b.DurationMinutes))
                    overlapPairs++;
            }
        }

        if (overlapPairs == 0)
            Pass("БД: нет пересечений занятий в залах");
        else
            Fail("БД: пересечения занятий", $"{overlapPairs} пар");

        var halls = await db.GymHalls.ToDictionaryAsync(h => h.IdGymHall, h => h.Capacity);
        var capacityViolations = classes.Count(g =>
            halls.TryGetValue(g.IdGymHall, out var cap) && g.MaxParticipants > cap);

        if (capacityViolations == 0)
            Pass("БД: лимиты участников в пределах залов");
        else
            Fail("БД: лимиты участников", $"{capacityViolations} занятий");

        var activeClasses = classes.Count;
        if (activeClasses == 0)
            Fail("Расписание", "нет активных занятий");
        else
            Pass($"Расписание ({activeClasses} занятий)");

        Console.WriteLine();
        if (failed == 0)
        {
            Console.WriteLine("ИТОГ: все тесты пройдены.");
            return 0;
        }

        Console.WriteLine($"ИТОГ: ошибок — {failed}");
        return 1;
    }

    private static DateOnly NextDateWithDayNumber(int dayNumber)
    {
        var date = DateOnly.FromDateTime(DateTime.Today);
        for (var i = 0; i < 8; i++)
        {
            var dn = date.DayOfWeek switch
            {
                System.DayOfWeek.Monday => 1,
                System.DayOfWeek.Tuesday => 2,
                System.DayOfWeek.Wednesday => 3,
                System.DayOfWeek.Thursday => 4,
                System.DayOfWeek.Friday => 5,
                System.DayOfWeek.Saturday => 6,
                System.DayOfWeek.Sunday => 7,
                _ => 0
            };
            if (dn == dayNumber) return date;
            date = date.AddDays(1);
        }

        return DateOnly.FromDateTime(DateTime.Today);
    }
}
