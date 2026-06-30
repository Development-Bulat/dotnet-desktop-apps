using System;
using System.Collections.Generic;

namespace Fitness_Club_01.Data;

public partial class MembershipStatus
{
    public int IdMembershipStatus { get; set; }

    public string StatusCode { get; set; } = null!;

    public string StatusName { get; set; } = null!;

    public virtual ICollection<MembershipStatusHistory> MembershipStatusHistories { get; set; } = new List<MembershipStatusHistory>();

    public virtual ICollection<Membership> Memberships { get; set; } = new List<Membership>();
}
