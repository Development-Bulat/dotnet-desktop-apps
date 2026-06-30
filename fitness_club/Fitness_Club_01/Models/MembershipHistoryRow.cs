namespace Fitness_Club_01.Models;

public class MembershipHistoryRow
{
    public int Number { get; set; }
    public string ChangedAt { get; set; } = "";
    public string StatusName { get; set; } = "";
    public string ChangedBy { get; set; } = "";
    public string Comment { get; set; } = "";
}
