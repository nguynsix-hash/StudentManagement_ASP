using System.ComponentModel.DataAnnotations;

namespace ConnectDB.DTOs;

public class ScheduleCreateDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime ScheduleDate { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [Required]
    public int TrainerId { get; set; }

    public int? MemberId { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

public class ScheduleUpdateDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime ScheduleDate { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [Required]
    public int TrainerId { get; set; }

    public int? MemberId { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

public class ScheduleResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime ScheduleDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int TrainerId { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public int? MemberId { get; set; }
    public string? MemberName { get; set; }
    public string? Notes { get; set; }
}
