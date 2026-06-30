namespace Fitness_Club_01.Models;

public class MembershipRow
{
    public int IdMembership { get; set; }
    public int Number { get; set; }
    public string ClientName { get; set; } = "";
    public string TypeName { get; set; } = "";
    public string StatusName { get; set; } = "";
    public string StatusCode { get; set; } = "";
    public string Period { get; set; } = "";
    public string Price { get; set; } = "";
    public int IdClient { get; set; }
}
