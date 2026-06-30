using System;
using System.Collections.Generic;

namespace Fitness_Club_01.Data;

public partial class UserRole
{
    public int IdUserRole { get; set; }

    public string RoleName { get; set; } = null!;

    public virtual ICollection<UserAccount> UserAccounts { get; set; } = new List<UserAccount>();
}
