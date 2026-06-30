namespace Fitness_Club_01.Models;

public class VisitRow
{
    public int IdVisit { get; set; }
    public int Number { get; set; }
    public string ClientName { get; set; } = "";
    public string VisitType { get; set; } = "";
    public string VisitDateTime { get; set; } = "";
    public string MarkedBy { get; set; } = "";
    public string MembershipType { get; set; } = "";
}
