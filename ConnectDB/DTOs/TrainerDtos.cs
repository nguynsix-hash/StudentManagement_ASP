using System.ComponentModel.DataAnnotations;

namespace ConnectDB.DTOs;

public class TrainerCreateDto
{
    [Required]
    [StringLength(30)]
    public string TrainerCode { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Specialty { get; set; }

    [StringLength(15)]
    public string? Phone { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }
}

public class TrainerUpdateDto
{
    [Required]
    [StringLength(30)]
    public string TrainerCode { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Specialty { get; set; }

    [StringLength(15)]
    public string? Phone { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    public bool IsActive { get; set; }
}

public class TrainerStatusUpdateDto
{
    public bool IsActive { get; set; }
}

public class TrainerResponseDto
{
    public int Id { get; set; }
    public string TrainerCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Specialty { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
