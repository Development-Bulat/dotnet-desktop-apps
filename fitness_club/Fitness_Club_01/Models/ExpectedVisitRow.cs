namespace Fitness_Club_01.Models;

public class ExpectedVisitRow
{
    public int IdClient { get; set; }
    public int? IdClassBooking { get; set; }
    public int Number { get; set; }
    public string ClientName { get; set; } = "";
    public string VisitType { get; set; } = "";
    public string ClassName { get; set; } = "";
    public string ClassTime { get; set; } = "";
    public string HallName { get; set; } = "";
    public string Status { get; set; } = "";
    public bool CanMark { get; set; }
}
