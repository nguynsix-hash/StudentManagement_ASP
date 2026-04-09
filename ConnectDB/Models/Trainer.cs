using System.ComponentModel.DataAnnotations;

namespace ConnectDB.Models;

public class Trainer
{
    [Key]
    public int Id { get; set; }

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

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
