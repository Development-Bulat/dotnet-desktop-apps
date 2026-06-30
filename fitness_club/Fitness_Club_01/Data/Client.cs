using System;
using System.Collections.Generic;

namespace Fitness_Club_01.Data;

public partial class Client
{
    public int IdClient { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? Patronymic { get; set; }

    public string Phone { get; set; } = null!;

    public DateOnly? BirthDate { get; set; }

    public DateTime RegisteredAt { get; set; }

    public virtual ICollection<ClassBooking> ClassBookings { get; set; } = new List<ClassBooking>();

    public virtual ICollection<Membership> Memberships { get; set; } = new List<Membership>();

    public virtual ICollection<UserAccount> UserAccounts { get; set; } = new List<UserAccount>();

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
