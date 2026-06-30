using System;
using System.Collections.Generic;

namespace Fitness_Club_01.Data;

public partial class GymHall
{
    public int IdGymHall { get; set; }

    public string HallName { get; set; } = null!;

    public int Capacity { get; set; }

    public virtual ICollection<GroupClass> GroupClasses { get; set; } = new List<GroupClass>();
}
