using System;
using System.Collections.Generic;

namespace Fitness_Club_01.Data;

public partial class GroupClass
{
    public int IdGroupClass { get; set; }

    public string ClassName { get; set; } = null!;

    public int IdGymHall { get; set; }

    public int IdTrainer { get; set; }

    public int IdDayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public int DurationMinutes { get; set; }

    public int MaxParticipants { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<ClassBooking> ClassBookings { get; set; } = new List<ClassBooking>();

    public virtual ICollection<CancelledClassSession> CancelledClassSessions { get; set; } = new List<CancelledClassSession>();

    public virtual DayOfWeek IdDayOfWeekNavigation { get; set; } = null!;

    public virtual GymHall IdGymHallNavigation { get; set; } = null!;

    public virtual Trainer IdTrainerNavigation { get; set; } = null!;
}
