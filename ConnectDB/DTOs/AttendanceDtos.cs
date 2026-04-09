using System.ComponentModel.DataAnnotations;

namespace ConnectDB.DTOs;

public class AttendanceMarkDto
{
    [Required]
    public int ScheduleId { get; set; }

    [Required]
    public int MemberId { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Present";

    [StringLength(300)]
    public string? Note { get; set; }
}

public class AttendanceUpdateDto
{
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Present";

    [StringLength(300)]
    public string? Note { get; set; }
}

public class AttendanceResponseDto
{
    public int Id { get; set; }
    public int ScheduleId { get; set; }
    public string ScheduleTitle { get; set; } = string.Empty;
    public DateTime ScheduleDate { get; set; }
    public int MemberId { get; set; }
    public string MemberCode { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime RecordedAt { get; set; }
}
