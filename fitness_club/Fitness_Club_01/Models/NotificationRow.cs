namespace Fitness_Club_01.Models;

public class NotificationRow
{
    public int IdNotification { get; set; }
    public int Number { get; set; }
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public string Status { get; set; } = "";
    public bool IsUnread { get; set; }
}
