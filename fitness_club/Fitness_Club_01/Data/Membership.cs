using System;
using System.Collections.Generic;

namespace Fitness_Club_01.Data;

public partial class Membership
{
    public int IdMembership { get; set; }

    public int IdClient { get; set; }

    public int IdMembershipType { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public int IdMembershipStatus { get; set; }

    public DateTime SoldAt { get; set; }

    public int? IdSoldByUser { get; set; }

    public virtual Client IdClientNavigation { get; set; } = null!;

    public virtual MembershipStatus IdMembershipStatusNavigation { get; set; } = null!;

    public virtual MembershipType IdMembershipTypeNavigation { get; set; } = null!;

    public virtual UserAccount? IdSoldByUserNavigation { get; set; }

    public virtual ICollection<MembershipStatusHistory> MembershipStatusHistories { get; set; } = new List<MembershipStatusHistory>();

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
