namespace Fitness_Club_01.Models;

public class BookingRow
{
    public int IdClassBooking { get; set; }
    public int Number { get; set; }
    public string ClientName { get; set; } = "";
    public string ClassName { get; set; } = "";
    public string ClassDate { get; set; } = "";
    public string ClassTime { get; set; } = "";
    public string ClassDateTime { get; set; } = "";
    public string TrainerName { get; set; } = "";
    public string HallName { get; set; } = "";
    public string BookedAt { get; set; } = "";
    public int IdClient { get; set; }
    public int IdGroupClass { get; set; }
    public DateOnly ClassDateValue { get; set; }
    public TimeOnly ClassStartTime { get; set; }
    public bool CanCancel { get; set; }
}
