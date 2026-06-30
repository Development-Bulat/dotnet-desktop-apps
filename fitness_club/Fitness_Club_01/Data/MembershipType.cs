using System;
using System.Collections.Generic;

namespace Fitness_Club_01.Data;

public partial class MembershipType
{
    public int IdMembershipType { get; set; }

    public string TypeName { get; set; } = null!;

    public decimal Price { get; set; }

    public int DurationDays { get; set; }

    public int? VisitLimit { get; set; }

    public virtual ICollection<Membership> Memberships { get; set; } = new List<Membership>();
}
