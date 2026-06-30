using Avalonia.Controls;
using Fitness_Club_01.Data;
using Fitness_Club_01.Services;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Controls;

public partial class DashboardControl : UserControl
{
    public DashboardControl()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadStatsAsync();
    }

    private async Task LoadStatsAsync()
    {
        await using var db = new ApplicationDbContext();
        await MembershipService.ExpireOutdatedAsync(db);

        ClientsCount.Text = (await db.Clients.CountAsync()).ToString();

        var activeStatus = await db.MembershipStatuses.FirstAsync(s => s.StatusCode == "active");
        ActiveMembershipsCount.Text = (await db.Memberships
            .CountAsync(m => m.IdMembershipStatus == activeStatus.IdMembershipStatus)).ToString();

        var expiredStatus = await db.MembershipStatuses.FirstAsync(s => s.StatusCode == "expired");
        ExpiredMembershipsCount.Text = (await db.Memberships
            .CountAsync(m => m.IdMembershipStatus == expiredStatus.IdMembershipStatus)).ToString();

        var today = DateOnly.FromDateTime(DateTime.Today);
        VisitsTodayCount.Text = (await db.Visits
            .CountAsync(v => DateOnly.FromDateTime(v.VisitDateTime) == today)).ToString();

        BookingsCount.Text = (await db.ClassBookings.CountAsync()).ToString();

        var monthStart = today.AddDays(-30);
        var sales = await db.Memberships
            .Include(m => m.IdMembershipTypeNavigation)
            .Where(m => DateOnly.FromDateTime(m.SoldAt) >= monthStart)
            .ToListAsync();
        RevenueMonthCount.Text = $"{sales.Sum(m => m.IdMembershipTypeNavigation.Price):N0} ₽";
    }
}
