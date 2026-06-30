using System;
using System.Collections.Generic;

namespace Fitness_Club_01.Data;

public partial class Staff
{
    public int IdStaff { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? Patronymic { get; set; }

    public string? Phone { get; set; }

    public DateOnly HiredAt { get; set; }

    public virtual ICollection<UserAccount> UserAccounts { get; set; } = new List<UserAccount>();
}
