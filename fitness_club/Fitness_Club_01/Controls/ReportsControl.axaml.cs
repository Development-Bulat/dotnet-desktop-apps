using Avalonia.Controls;
using Avalonia.VisualTree;
using Fitness_Club_01.Data;
using Fitness_Club_01.Services;
using Fitness_Club_01.Views;

namespace Fitness_Club_01.Controls;

public partial class ReportsControl : UserControl
{
    public ReportsControl()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            var today = DateTime.Today;
            FromDatePicker.SelectedDate = today.AddDays(-30);
            ToDatePicker.SelectedDate = today;
        };

        VisitsReportBtn.Click += async (_, _) => await ShowReportAsync("Отчёт по визитам", BuildVisits);
        SalesReportBtn.Click += async (_, _) => await ShowReportAsync("Отчёт по продажам", BuildSales);
        ExpiringReportBtn.Click += async (_, _) => await ShowExpiringAsync();
    }

    private Window? GetOwner() => TopLevel.GetTopLevel(this) as Window;

    private bool TryGetPeriod(out DateOnly from, out DateOnly to)
    {
        from = default;
        to = default;

        if (!FromDatePicker.SelectedDate.HasValue || !ToDatePicker.SelectedDate.HasValue)
        {
            StatusText.Text = "Укажите период";
            return false;
        }

        from = DateOnly.FromDateTime(FromDatePicker.SelectedDate.Value.DateTime);
        to = DateOnly.FromDateTime(ToDatePicker.SelectedDate.Value.DateTime);

        if (to < from)
        {
            StatusText.Text = "Дата «По» не может быть раньше «С»";
            return false;
        }

        return true;
    }

    private async Task ShowReportAsync(string title, Func<ApplicationDbContext, DateOnly, DateOnly, Task<string>> builder)
    {
        if (!TryGetPeriod(out var from, out var to)) return;

        await using var db = new ApplicationDbContext();
        var text = await builder(db, from, to);
        var owner = GetOwner();
        if (owner == null) return;

        var win = new ReportPreviewWindow(title, text);
        await win.ShowDialog(owner);
        StatusText.Text = "Отчёт сформирован";
    }

    private Task<string> BuildVisits(ApplicationDbContext db, DateOnly from, DateOnly to) =>
        ReportService.BuildVisitsReportAsync(db, from, to);

    private Task<string> BuildSales(ApplicationDbContext db, DateOnly from, DateOnly to) =>
        ReportService.BuildSalesReportAsync(db, from, to);

    private async Task ShowExpiringAsync()
    {
        await using var db = new ApplicationDbContext();
        var text = await ReportService.BuildExpiringReportAsync(db);
        var owner = GetOwner();
        if (owner == null) return;

        var win = new ReportPreviewWindow("Истекающие абонементы", text);
        await win.ShowDialog(owner);
        StatusText.Text = "Отчёт сформирован";
    }
}
