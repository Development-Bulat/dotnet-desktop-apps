namespace Fitness_Club_01.Models;

public class GroupClassRow
{
    public int IdGroupClass { get; set; }
    public int Number { get; set; }
    public string ClassName { get; set; } = "";
    public string HallName { get; set; } = "";
    public string TrainerName { get; set; } = "";
    public string Schedule { get; set; } = "";
    public string MaxParticipants { get; set; } = "";
    public string IsActive { get; set; } = "";
}
