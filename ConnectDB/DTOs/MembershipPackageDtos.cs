using System.ComponentModel.DataAnnotations;

namespace ConnectDB.DTOs;

public class MembershipPackageCreateDto
{
    [Required]
    [StringLength(30)]
    public string PackageCode { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(1, 3650)]
    public int DurationDays { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
}

public class MembershipPackageUpdateDto
{
    [Required]
    [StringLength(30)]
    public string PackageCode { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(1, 3650)]
    public int DurationDays { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    public bool IsActive { get; set; }
}

public class MembershipPackageStatusUpdateDto
{
    public bool IsActive { get; set; }
}

public class MembershipPackageResponseDto
{
    public int Id { get; set; }
    public string PackageCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationDays { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
