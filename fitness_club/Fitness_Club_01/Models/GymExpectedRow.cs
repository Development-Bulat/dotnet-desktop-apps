namespace Fitness_Club_01.Models;

public class GymExpectedRow
{
    public int IdClient { get; set; }
    public int Number { get; set; }
    public string ClientName { get; set; } = "";
    public string MembershipType { get; set; } = "";
    public string Status { get; set; } = "";
    public bool CanMark { get; set; }
}
