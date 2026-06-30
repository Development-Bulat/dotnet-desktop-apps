using System;

namespace Fitness_Club_01.Data;

public partial class CancelledClassSession
{
    public int IdCancelledClassSession { get; set; }

    public int IdGroupClass { get; set; }

    public DateOnly ClassDate { get; set; }

    public DateTime CancelledAt { get; set; }

    public int IdCancelledByUser { get; set; }

    public virtual GroupClass IdGroupClassNavigation { get; set; } = null!;

    public virtual UserAccount IdCancelledByUserNavigation { get; set; } = null!;
}
