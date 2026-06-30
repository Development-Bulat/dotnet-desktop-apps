using System;

namespace Fitness_Club_01.Data;

public partial class Notification
{
    public int IdNotification { get; set; }

    public int IdUserAccount { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsRead { get; set; }

    public virtual UserAccount IdUserAccountNavigation { get; set; } = null!;
}
