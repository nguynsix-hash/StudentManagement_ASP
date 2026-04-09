using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectDB.Models;

public class Payment
{
    [Key]
    public int Id { get; set; }

    public int SubscriptionId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.Now;

    [StringLength(30)]
    public string PaymentMethod { get; set; } = "Cash";

    [StringLength(20)]
    public string Status { get; set; } = "Pending";

    [StringLength(500)]
    public string? Note { get; set; }

    public Subscription? Subscription { get; set; }
}
