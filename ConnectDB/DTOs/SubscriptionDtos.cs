using System.ComponentModel.DataAnnotations;

namespace ConnectDB.DTOs;

public class SubscriptionCreateDto
{
    [Required]
    public int MemberId { get; set; }

    [Required]
    public int MembershipPackageId { get; set; }

    public DateTime? StartDate { get; set; }
}

public class SubscriptionExtendDto
{
    [Range(1, 3650)]
    public int? ExtraDays { get; set; }
}

public class SubscriptionStatusUpdateDto
{
    [Required]
    [StringLength(20)]
    [RegularExpression("^(Active|Expired|Cancelled)$", ErrorMessage = "Status must be Active, Expired, or Cancelled.")]
    public string Status { get; set; } = "Active";
}

public class SubscriptionStatusDto
{
    public int SubscriptionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class SubscriptionResponseDto
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string MemberCode { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public int MembershipPackageId { get; set; }
    public string PackageCode { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public int PackageDurationDays { get; set; }
    public decimal PackagePrice { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
