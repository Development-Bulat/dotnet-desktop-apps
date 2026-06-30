using System;
using System.Collections.Generic;

namespace Fitness_Club_01.Data;

public partial class UserAccount
{
    public int IdUserAccount { get; set; }

    public string Login { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int IdUserRole { get; set; }

    public int? IdClient { get; set; }

    public int? IdTrainer { get; set; }

    public int? IdStaff { get; set; }

    public virtual ICollection<ClassBooking> ClassBookings { get; set; } = new List<ClassBooking>();

    public virtual Client? IdClientNavigation { get; set; }

    public virtual Staff? IdStaffNavigation { get; set; }

    public virtual Trainer? IdTrainerNavigation { get; set; }

    public virtual UserRole IdUserRoleNavigation { get; set; } = null!;

    public virtual ICollection<MembershipStatusHistory> MembershipStatusHistories { get; set; } = new List<MembershipStatusHistory>();

    public virtual ICollection<Membership> Memberships { get; set; } = new List<Membership>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<CancelledClassSession> CancelledClassSessions { get; set; } = new List<CancelledClassSession>();

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
