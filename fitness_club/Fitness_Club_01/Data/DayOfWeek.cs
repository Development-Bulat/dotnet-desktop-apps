using System;
using System.Collections.Generic;

namespace Fitness_Club_01.Data;

public partial class DayOfWeek
{
    public int IdDayOfWeek { get; set; }

    public string DayName { get; set; } = null!;

    public short DayNumber { get; set; }

    public virtual ICollection<GroupClass> GroupClasses { get; set; } = new List<GroupClass>();
}
