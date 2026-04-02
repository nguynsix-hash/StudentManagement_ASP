using System.ComponentModel.DataAnnotations;

namespace ConnectDB.Models;

public class Vendor
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string VendorCode { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string VendorName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? TaxNo { get; set; }

    [StringLength(200)]
    public string? PaymentTerms { get; set; }
}
