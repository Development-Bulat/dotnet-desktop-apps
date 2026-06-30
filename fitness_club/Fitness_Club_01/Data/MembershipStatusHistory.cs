using System;
using System.Collections.Generic;

namespace Fitness_Club_01.Data;

public partial class MembershipStatusHistory
{
    public int IdMembershipStatusHistory { get; set; }

    public int IdMembership { get; set; }

    public int IdMembershipStatus { get; set; }

    public DateTime ChangedAt { get; set; }

    public int? IdChangedByUser { get; set; }

    public string? Comment { get; set; }

    public virtual UserAccount? IdChangedByUserNavigation { get; set; }

    public virtual Membership IdMembershipNavigation { get; set; } = null!;

    public virtual MembershipStatus IdMembershipStatusNavigation { get; set; } = null!;
}
