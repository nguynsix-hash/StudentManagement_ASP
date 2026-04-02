using System.ComponentModel.DataAnnotations;

namespace ConnectDB.DTOs;

public class VendorDto
{
    public int Id { get; set; }
    public string VendorCode { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public string? TaxNo { get; set; }
    public string? PaymentTerms { get; set; }
}

public class CreateVendorDto
{
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

public class UpdateVendorDto
{
    [StringLength(50)]
    public string VendorCode { get; set; } = string.Empty;

    [StringLength(200)]
    public string VendorName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? TaxNo { get; set; }

    [StringLength(200)]
    public string? PaymentTerms { get; set; }
}
