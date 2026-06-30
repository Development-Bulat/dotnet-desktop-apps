using System;
using System.Collections.Generic;

namespace Fitness_Club_01.Data;

public partial class Visit
{
    public int IdVisit { get; set; }

    public int IdClient { get; set; }

    public DateTime VisitDateTime { get; set; }

    public int IdMarkedByUser { get; set; }

    public int? IdMembership { get; set; }

    public virtual Client IdClientNavigation { get; set; } = null!;

    public virtual UserAccount IdMarkedByUserNavigation { get; set; } = null!;

    public virtual Membership? IdMembershipNavigation { get; set; }
}
