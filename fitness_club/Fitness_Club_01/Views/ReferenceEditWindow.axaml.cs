using Avalonia.Controls;
using Avalonia.Interactivity;
using Fitness_Club_01.Data;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Views;

public enum ReferenceKind
{
    GymHall,
    MembershipType,
    Specialization
}

public partial class ReferenceEditWindow : Window
{
    private readonly ApplicationDbContext _db = new();
    private readonly ReferenceKind _kind;
    private readonly int? _id;

    public bool Saved { get; private set; }

    public ReferenceEditWindow(ReferenceKind kind, int? id = null)
    {
        _kind = kind;
        _id = id;
        InitializeComponent();
        SaveButton.Click += OnSaveClick;
        CancelButton.Click += (_, _) => Close();
        Loaded += async (_, _) => await LoadAsync();
        Closed += (_, _) => _db.Dispose();

        PriceBox.IsVisible = _kind == ReferenceKind.MembershipType;
        DurationBox.IsVisible = _kind == ReferenceKind.MembershipType;
        VisitLimitBox.IsVisible = _kind == ReferenceKind.MembershipType;
        CapacityBox.IsVisible = _kind == ReferenceKind.GymHall;

        TitleText.Text = _kind switch
        {
            ReferenceKind.GymHall => id == null ? "Новый зал" : "Редактирование зала",
            ReferenceKind.MembershipType => id == null ? "Новый тариф" : "Редактирование тарифа",
            ReferenceKind.Specialization => id == null ? "Новая специализация" : "Редактирование специализации",
            _ => "Справочник"
        };
    }

    private async Task LoadAsync()
    {
        if (_id == null) return;

        switch (_kind)
        {
            case ReferenceKind.GymHall:
                var hall = await _db.GymHalls.FindAsync(_id);
                if (hall != null)
                {
                    NameBox.Text = hall.HallName;
                    CapacityBox.Text = hall.Capacity.ToString();
                }
                break;
            case ReferenceKind.MembershipType:
                var type = await _db.MembershipTypes.FindAsync(_id);
                if (type != null)
                {
                    NameBox.Text = type.TypeName;
                    PriceBox.Text = type.Price.ToString("0.##");
                    DurationBox.Text = type.DurationDays.ToString();
                    VisitLimitBox.Text = type.VisitLimit?.ToString() ?? "";
                }
                break;
            case ReferenceKind.Specialization:
                var spec = await _db.Specializations.FindAsync(_id);
                if (spec != null)
                    NameBox.Text = spec.SpecializationName;
                break;
        }
    }

    private async void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        var name = NameBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            ErrorText.Text = "Укажите название";
            return;
        }

        try
        {
            switch (_kind)
            {
                case ReferenceKind.GymHall:
                    if (!int.TryParse(CapacityBox.Text?.Trim(), out var capacity) || capacity <= 0)
                    {
                        ErrorText.Text = "Укажите вместимость";
                        return;
                    }

                    GymHall hall;
                    if (_id.HasValue)
                        hall = await _db.GymHalls.FirstAsync(h => h.IdGymHall == _id.Value);
                    else
                    {
                        hall = new GymHall();
                        _db.GymHalls.Add(hall);
                    }

                    hall.HallName = name;
                    hall.Capacity = capacity;
                    break;

                case ReferenceKind.MembershipType:
                    if (!decimal.TryParse(PriceBox.Text?.Trim(), out var price) || price < 0)
                    {
                        ErrorText.Text = "Укажите цену";
                        return;
                    }

                    if (!int.TryParse(DurationBox.Text?.Trim(), out var duration) || duration <= 0)
                    {
                        ErrorText.Text = "Укажите срок в днях";
                        return;
                    }

                    int? visitLimit = null;
                    if (!string.IsNullOrWhiteSpace(VisitLimitBox.Text))
                    {
                        if (!int.TryParse(VisitLimitBox.Text.Trim(), out var limit) || limit <= 0)
                        {
                            ErrorText.Text = "Некорректный лимит визитов";
                            return;
                        }

                        visitLimit = limit;
                    }

                    MembershipType type;
                    if (_id.HasValue)
                        type = await _db.MembershipTypes.FirstAsync(t => t.IdMembershipType == _id.Value);
                    else
                    {
                        type = new MembershipType();
                        _db.MembershipTypes.Add(type);
                    }

                    type.TypeName = name;
                    type.Price = price;
                    type.DurationDays = duration;
                    type.VisitLimit = visitLimit;
                    break;

                case ReferenceKind.Specialization:
                    Specialization spec;
                    if (_id.HasValue)
                        spec = await _db.Specializations.FirstAsync(s => s.IdSpecialization == _id.Value);
                    else
                    {
                        spec = new Specialization();
                        _db.Specializations.Add(spec);
                    }

                    spec.SpecializationName = name;
                    break;
            }

            await _db.SaveChangesAsync();
            Saved = true;
            Close();
        }
        catch (Exception ex)
        {
            ErrorText.Text = ex.InnerException?.Message ?? ex.Message;
        }
    }
}
