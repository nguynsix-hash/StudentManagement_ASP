using System.ComponentModel.DataAnnotations;

namespace ConnectDB.DTOs;

public class PaymentCreateDto
{
    [Required]
    public int SubscriptionId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    public DateTime? PaymentDate { get; set; }

    [StringLength(30)]
    public string PaymentMethod { get; set; } = "Cash";

    [StringLength(20)]
    public string Status { get; set; } = "Paid";

    [StringLength(500)]
    public string? Note { get; set; }
}

public class PaymentUpdateDto
{
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    [StringLength(30)]
    public string PaymentMethod { get; set; } = "Cash";

    [StringLength(20)]
    public string Status { get; set; } = "Paid";

    [StringLength(500)]
    public string? Note { get; set; }
}

public class PaymentResponseDto
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public int MemberId { get; set; }
    public string MemberCode { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public int MembershipPackageId { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
}
