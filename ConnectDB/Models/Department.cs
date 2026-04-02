using System.ComponentModel.DataAnnotations;

namespace ConnectDB.Models;

public class Department
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string DeptCode { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string DeptName { get; set; } = string.Empty;
}
