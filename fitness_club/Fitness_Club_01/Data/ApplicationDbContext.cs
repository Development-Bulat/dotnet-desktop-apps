using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Fitness_Club_01.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ClassBooking> ClassBookings { get; set; }

    public virtual DbSet<CancelledClassSession> CancelledClassSessions { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<DayOfWeek> DayOfWeeks { get; set; }

    public virtual DbSet<GroupClass> GroupClasses { get; set; }

    public virtual DbSet<GymHall> GymHalls { get; set; }

    public virtual DbSet<Membership> Memberships { get; set; }

    public virtual DbSet<MembershipStatus> MembershipStatuses { get; set; }

    public virtual DbSet<MembershipStatusHistory> MembershipStatusHistories { get; set; }

    public virtual DbSet<MembershipType> MembershipTypes { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Specialization> Specializations { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<Trainer> Trainers { get; set; }

    public virtual DbSet<UserAccount> UserAccounts { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<Visit> Visits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("C");

        modelBuilder.Entity<ClassBooking>(entity =>
        {
            entity.HasKey(e => e.IdClassBooking).HasName("ClassBooking_pkey");

            entity.ToTable("ClassBooking");

            entity.HasIndex(e => new { e.IdClient, e.IdGroupClass, e.ClassDate }, "ClassBooking_client_class_date_key").IsUnique();

            entity.HasIndex(e => new { e.IdGroupClass, e.ClassDate }, "IX_ClassBooking_GroupClass_Date");

            entity.Property(e => e.IdClassBooking).UseIdentityAlwaysColumn();
            entity.Property(e => e.BookedAt)
                .HasDefaultValueSql("(now() AT TIME ZONE 'UTC'::text)")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.IdBookedByUserNavigation).WithMany(p => p.ClassBookings)
                .HasForeignKey(d => d.IdBookedByUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClassBooking_BookedByUser");

            entity.HasOne(d => d.IdClientNavigation).WithMany(p => p.ClassBookings)
                .HasForeignKey(d => d.IdClient)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClassBooking_Client");

            entity.HasOne(d => d.IdGroupClassNavigation).WithMany(p => p.ClassBookings)
                .HasForeignKey(d => d.IdGroupClass)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ClassBooking_GroupClass");
        });

        modelBuilder.Entity<CancelledClassSession>(entity =>
        {
            entity.HasKey(e => e.IdCancelledClassSession).HasName("CancelledClassSession_pkey");

            entity.ToTable("CancelledClassSession");

            entity.HasIndex(e => new { e.IdGroupClass, e.ClassDate }, "CancelledClassSession_class_date_key").IsUnique();

            entity.HasIndex(e => e.ClassDate, "IX_CancelledClassSession_Date");

            entity.Property(e => e.IdCancelledClassSession).UseIdentityAlwaysColumn();
            entity.Property(e => e.CancelledAt)
                .HasDefaultValueSql("(now() AT TIME ZONE 'UTC'::text)")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.IdGroupClassNavigation).WithMany(p => p.CancelledClassSessions)
                .HasForeignKey(d => d.IdGroupClass)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_CancelledClassSession_GroupClass");

            entity.HasOne(d => d.IdCancelledByUserNavigation).WithMany(p => p.CancelledClassSessions)
                .HasForeignKey(d => d.IdCancelledByUser)
                .HasConstraintName("FK_CancelledClassSession_User");
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.IdClient).HasName("Client_pkey");

            entity.ToTable("Client");

            entity.HasIndex(e => e.Phone, "Client_Phone_key").IsUnique();

            entity.Property(e => e.IdClient).UseIdentityAlwaysColumn();
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Patronymic).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.RegisteredAt)
                .HasDefaultValueSql("(now() AT TIME ZONE 'UTC'::text)")
                .HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<DayOfWeek>(entity =>
        {
            entity.HasKey(e => e.IdDayOfWeek).HasName("DayOfWeek_pkey");

            entity.ToTable("DayOfWeek");

            entity.HasIndex(e => e.DayName, "DayOfWeek_DayName_key").IsUnique();

            entity.HasIndex(e => e.DayNumber, "DayOfWeek_DayNumber_key").IsUnique();

            entity.Property(e => e.IdDayOfWeek).UseIdentityAlwaysColumn();
            entity.Property(e => e.DayName).HasMaxLength(20);
        });

        modelBuilder.Entity<GroupClass>(entity =>
        {
            entity.HasKey(e => e.IdGroupClass).HasName("GroupClass_pkey");

            entity.ToTable("GroupClass");

            entity.Property(e => e.IdGroupClass).UseIdentityAlwaysColumn();
            entity.Property(e => e.ClassName).HasMaxLength(150);
            entity.Property(e => e.DurationMinutes).HasDefaultValue(60);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.IdDayOfWeekNavigation).WithMany(p => p.GroupClasses)
                .HasForeignKey(d => d.IdDayOfWeek)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GroupClass_DayOfWeek");

            entity.HasOne(d => d.IdGymHallNavigation).WithMany(p => p.GroupClasses)
                .HasForeignKey(d => d.IdGymHall)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GroupClass_GymHall");

            entity.HasOne(d => d.IdTrainerNavigation).WithMany(p => p.GroupClasses)
                .HasForeignKey(d => d.IdTrainer)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GroupClass_Trainer");
        });

        modelBuilder.Entity<GymHall>(entity =>
        {
            entity.HasKey(e => e.IdGymHall).HasName("GymHall_pkey");

            entity.ToTable("GymHall");

            entity.HasIndex(e => e.HallName, "GymHall_HallName_key").IsUnique();

            entity.Property(e => e.IdGymHall).UseIdentityAlwaysColumn();
            entity.Property(e => e.HallName).HasMaxLength(100);
        });

        modelBuilder.Entity<Membership>(entity =>
        {
            entity.HasKey(e => e.IdMembership).HasName("Membership_pkey");

            entity.ToTable("Membership");

            entity.HasIndex(e => e.IdClient, "IX_Membership_Client");

            entity.HasIndex(e => e.IdMembershipStatus, "IX_Membership_Status");

            entity.Property(e => e.IdMembership).UseIdentityAlwaysColumn();
            entity.Property(e => e.SoldAt)
                .HasDefaultValueSql("(now() AT TIME ZONE 'UTC'::text)")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.IdClientNavigation).WithMany(p => p.Memberships)
                .HasForeignKey(d => d.IdClient)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Membership_Client");

            entity.HasOne(d => d.IdMembershipStatusNavigation).WithMany(p => p.Memberships)
                .HasForeignKey(d => d.IdMembershipStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Membership_MembershipStatus");

            entity.HasOne(d => d.IdMembershipTypeNavigation).WithMany(p => p.Memberships)
                .HasForeignKey(d => d.IdMembershipType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Membership_MembershipType");

            entity.HasOne(d => d.IdSoldByUserNavigation).WithMany(p => p.Memberships)
                .HasForeignKey(d => d.IdSoldByUser)
                .HasConstraintName("FK_Membership_SoldByUser");
        });

        modelBuilder.Entity<MembershipStatus>(entity =>
        {
            entity.HasKey(e => e.IdMembershipStatus).HasName("MembershipStatus_pkey");

            entity.ToTable("MembershipStatus");

            entity.HasIndex(e => e.StatusCode, "MembershipStatus_StatusCode_key").IsUnique();

            entity.HasIndex(e => e.StatusName, "MembershipStatus_StatusName_key").IsUnique();

            entity.Property(e => e.IdMembershipStatus).UseIdentityAlwaysColumn();
            entity.Property(e => e.StatusCode).HasMaxLength(30);
            entity.Property(e => e.StatusName).HasMaxLength(100);
        });

        modelBuilder.Entity<MembershipStatusHistory>(entity =>
        {
            entity.HasKey(e => e.IdMembershipStatusHistory).HasName("MembershipStatusHistory_pkey");

            entity.ToTable("MembershipStatusHistory");

            entity.Property(e => e.IdMembershipStatusHistory).UseIdentityAlwaysColumn();
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("(now() AT TIME ZONE 'UTC'::text)")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.IdChangedByUserNavigation).WithMany(p => p.MembershipStatusHistories)
                .HasForeignKey(d => d.IdChangedByUser)
                .HasConstraintName("FK_MembershipStatusHistory_User");

            entity.HasOne(d => d.IdMembershipNavigation).WithMany(p => p.MembershipStatusHistories)
                .HasForeignKey(d => d.IdMembership)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_MembershipStatusHistory_Membership");

            entity.HasOne(d => d.IdMembershipStatusNavigation).WithMany(p => p.MembershipStatusHistories)
                .HasForeignKey(d => d.IdMembershipStatus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MembershipStatusHistory_MembershipStatus");
        });

        modelBuilder.Entity<MembershipType>(entity =>
        {
            entity.HasKey(e => e.IdMembershipType).HasName("MembershipType_pkey");

            entity.ToTable("MembershipType");

            entity.HasIndex(e => e.TypeName, "MembershipType_TypeName_key").IsUnique();

            entity.Property(e => e.IdMembershipType).UseIdentityAlwaysColumn();
            entity.Property(e => e.Price).HasPrecision(10, 2);
            entity.Property(e => e.TypeName).HasMaxLength(100);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.IdNotification).HasName("Notification_pkey");

            entity.ToTable("Notification");

            entity.HasIndex(e => new { e.IdUserAccount, e.IsRead }, "IX_Notification_UserAccount_Read");

            entity.HasIndex(e => new { e.IdUserAccount, e.CreatedAt }, "IX_Notification_UserAccount_CreatedAt");

            entity.Property(e => e.IdNotification).UseIdentityAlwaysColumn();
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(now() AT TIME ZONE 'UTC'::text)")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsRead).HasDefaultValue(false);

            entity.HasOne(d => d.IdUserAccountNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.IdUserAccount)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Notification_UserAccount");
        });

        modelBuilder.Entity<Specialization>(entity =>
        {
            entity.HasKey(e => e.IdSpecialization).HasName("Specialization_pkey");

            entity.ToTable("Specialization");

            entity.HasIndex(e => e.SpecializationName, "Specialization_SpecializationName_key").IsUnique();

            entity.Property(e => e.IdSpecialization).UseIdentityAlwaysColumn();
            entity.Property(e => e.SpecializationName).HasMaxLength(100);
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.IdStaff).HasName("Staff_pkey");

            entity.HasIndex(e => e.Phone, "Staff_Phone_key").IsUnique();

            entity.Property(e => e.IdStaff).UseIdentityAlwaysColumn();
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.HiredAt).HasDefaultValueSql("CURRENT_DATE");
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Patronymic).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<Trainer>(entity =>
        {
            entity.HasKey(e => e.IdTrainer).HasName("Trainer_pkey");

            entity.ToTable("Trainer");

            entity.HasIndex(e => e.Phone, "Trainer_Phone_key").IsUnique();

            entity.Property(e => e.IdTrainer).UseIdentityAlwaysColumn();
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.HiredAt).HasDefaultValueSql("CURRENT_DATE");
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Patronymic).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);

            entity.HasMany(d => d.IdSpecializations).WithMany(p => p.IdTrainers)
                .UsingEntity<Dictionary<string, object>>(
                    "TrainerSpecialization",
                    r => r.HasOne<Specialization>().WithMany()
                        .HasForeignKey("IdSpecialization")
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("FK_TrainerSpecialization_Specialization"),
                    l => l.HasOne<Trainer>().WithMany()
                        .HasForeignKey("IdTrainer")
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("FK_TrainerSpecialization_Trainer"),
                    j =>
                    {
                        j.HasKey("IdTrainer", "IdSpecialization").HasName("TrainerSpecialization_pkey");
                        j.ToTable("TrainerSpecialization");
                    });
        });

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(e => e.IdUserAccount).HasName("UserAccount_pkey");

            entity.ToTable("UserAccount");

            entity.HasIndex(e => e.IdUserRole, "IX_UserAccount_Role");

            entity.HasIndex(e => e.Login, "UserAccount_Login_key").IsUnique();

            entity.Property(e => e.IdUserAccount).UseIdentityAlwaysColumn();
            entity.Property(e => e.Login).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);

            entity.HasOne(d => d.IdClientNavigation).WithMany(p => p.UserAccounts)
                .HasForeignKey(d => d.IdClient)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_UserAccount_Client");

            entity.HasOne(d => d.IdStaffNavigation).WithMany(p => p.UserAccounts)
                .HasForeignKey(d => d.IdStaff)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_UserAccount_Staff");

            entity.HasOne(d => d.IdTrainerNavigation).WithMany(p => p.UserAccounts)
                .HasForeignKey(d => d.IdTrainer)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_UserAccount_Trainer");

            entity.HasOne(d => d.IdUserRoleNavigation).WithMany(p => p.UserAccounts)
                .HasForeignKey(d => d.IdUserRole)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAccount_UserRole");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.IdUserRole).HasName("UserRole_pkey");

            entity.ToTable("UserRole");

            entity.HasIndex(e => e.RoleName, "UserRole_RoleName_key").IsUnique();

            entity.Property(e => e.IdUserRole).UseIdentityAlwaysColumn();
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Visit>(entity =>
        {
            entity.HasKey(e => e.IdVisit).HasName("Visit_pkey");

            entity.ToTable("Visit");

            entity.HasIndex(e => new { e.IdClient, e.VisitDateTime }, "IX_Visit_Client_DateTime");

            entity.Property(e => e.IdVisit).UseIdentityAlwaysColumn();
            entity.Property(e => e.VisitDateTime)
                .HasDefaultValueSql("(now() AT TIME ZONE 'UTC'::text)")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.IdClientNavigation).WithMany(p => p.Visits)
                .HasForeignKey(d => d.IdClient)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Visit_Client");

            entity.HasOne(d => d.IdMarkedByUserNavigation).WithMany(p => p.Visits)
                .HasForeignKey(d => d.IdMarkedByUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Visit_MarkedByUser");

            entity.HasOne(d => d.IdMembershipNavigation).WithMany(p => p.Visits)
                .HasForeignKey(d => d.IdMembership)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Visit_Membership");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
