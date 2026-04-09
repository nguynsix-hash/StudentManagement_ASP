using System.ComponentModel.DataAnnotations;

namespace ConnectDB.Models;

public class Schedule
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public DateTime ScheduleDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public int TrainerId { get; set; }

    public int? MemberId { get; set; }

    public Trainer? Trainer { get; set; }

    public Member? Member { get; set; }

    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
