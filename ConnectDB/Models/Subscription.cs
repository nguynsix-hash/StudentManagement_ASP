using System.ComponentModel.DataAnnotations;

namespace ConnectDB.Models;

public class Subscription
{
    [Key]
    public int Id { get; set; }

    public int MemberId { get; set; }

    public int MembershipPackageId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Active";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Member? Member { get; set; }

    public MembershipPackage? MembershipPackage { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
