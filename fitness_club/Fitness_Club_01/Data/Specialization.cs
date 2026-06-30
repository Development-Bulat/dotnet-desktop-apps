using System;
using System.Collections.Generic;

namespace Fitness_Club_01.Data;

public partial class Specialization
{
    public int IdSpecialization { get; set; }

    public string SpecializationName { get; set; } = null!;

    public virtual ICollection<Trainer> IdTrainers { get; set; } = new List<Trainer>();

    public override string ToString() => SpecializationName;
}
