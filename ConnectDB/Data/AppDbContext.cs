using ConnectDB.Models;
using Microsoft.EntityFrameworkCore;

namespace ConnectDB.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Trainer> Trainers => Set<Trainer>();
    public DbSet<MembershipPackage> MembershipPackages => Set<MembershipPackage>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();

        modelBuilder.Entity<Account>()
            .HasIndex(a => a.Username)
            .IsUnique();

        modelBuilder.Entity<Member>()
            .HasIndex(m => m.MemberCode)
            .IsUnique();

        modelBuilder.Entity<Trainer>()
            .HasIndex(t => t.TrainerCode)
            .IsUnique();

        modelBuilder.Entity<MembershipPackage>()
            .HasIndex(p => p.PackageCode)
            .IsUnique();

        modelBuilder.Entity<Attendance>()
            .HasIndex(a => new { a.ScheduleId, a.MemberId })
            .IsUnique();

        modelBuilder.Entity<Account>()
            .HasOne(a => a.Role)
            .WithMany(r => r.Accounts)
            .HasForeignKey(a => a.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Subscription>()
            .HasOne(s => s.Member)
            .WithMany(m => m.Subscriptions)
            .HasForeignKey(s => s.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Subscription>()
            .HasOne(s => s.MembershipPackage)
            .WithMany(p => p.Subscriptions)
            .HasForeignKey(s => s.MembershipPackageId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Schedule>()
            .HasOne(s => s.Trainer)
            .WithMany(t => t.Schedules)
            .HasForeignKey(s => s.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Schedule>()
            .HasOne(s => s.Member)
            .WithMany(m => m.Schedules)
            .HasForeignKey(s => s.MemberId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Attendance>()
            .HasOne(a => a.Schedule)
            .WithMany(s => s.Attendances)
            .HasForeignKey(a => a.ScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Attendance>()
            .HasOne(a => a.Member)
            .WithMany(m => m.Attendances)
            .HasForeignKey(a => a.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Subscription)
            .WithMany(s => s.Payments)
            .HasForeignKey(p => p.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin", Description = "Quản trị hệ thống" },
            new Role { Id = 2, Name = "Receptionist", Description = "Nhân viên lễ tân / quản lý" },
            new Role { Id = 3, Name = "Trainer", Description = "Huấn luyện viên" },
            new Role { Id = 4, Name = "Member", Description = "Hội viên" }
        );
    }
}
