using System.ComponentModel.DataAnnotations;

namespace ConnectDB.Models;

public class Attendance
{
    [Key]
    public int Id { get; set; }

    public int ScheduleId { get; set; }

    public int MemberId { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Present";

    [StringLength(300)]
    public string? Note { get; set; }

    public DateTime RecordedAt { get; set; } = DateTime.Now;

    public Schedule? Schedule { get; set; }

    public Member? Member { get; set; }
}
