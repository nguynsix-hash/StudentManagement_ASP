using System.ComponentModel.DataAnnotations;

namespace ConnectDB.Models;

public class Member
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(30)]
    public string MemberCode { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    [StringLength(20)]
    public string? Gender { get; set; }

    [StringLength(15)]
    public string? Phone { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
