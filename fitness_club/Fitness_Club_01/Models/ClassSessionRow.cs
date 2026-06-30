namespace Fitness_Club_01.Models;

public class ClassSessionRow
{
    public int IdGroupClass { get; set; }
    public int Number { get; set; }
    public DateOnly ClassDateValue { get; set; }
    public string ClassDate { get; set; } = "";
    public string ClassTime { get; set; } = "";
    public string ClassName { get; set; } = "";
    public string HallName { get; set; } = "";
    public string BookedCount { get; set; } = "";
    public string Status { get; set; } = "";
    public bool IsCancelled { get; set; }
    public bool CanCancel { get; set; }
}
