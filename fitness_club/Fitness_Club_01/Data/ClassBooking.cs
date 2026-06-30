using System;
using System.Collections.Generic;

namespace Fitness_Club_01.Data;

public partial class ClassBooking
{
    public int IdClassBooking { get; set; }

    public int IdClient { get; set; }

    public int IdGroupClass { get; set; }

    public DateOnly ClassDate { get; set; }

    public DateTime BookedAt { get; set; }

    public int IdBookedByUser { get; set; }

    public virtual UserAccount IdBookedByUserNavigation { get; set; } = null!;

    public virtual Client IdClientNavigation { get; set; } = null!;

    public virtual GroupClass IdGroupClassNavigation { get; set; } = null!;
}
