using System.ComponentModel.DataAnnotations;

namespace ConnectDB.DTOs;

public class DepartmentDto
{
    public int Id { get; set; }
    public string DeptCode { get; set; } = string.Empty;
    public string DeptName { get; set; } = string.Empty;
}

public class CreateDepartmentDto
{
    [Required]
    [StringLength(50)]
    public string DeptCode { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string DeptName { get; set; } = string.Empty;
}

public class UpdateDepartmentDto
{
    [StringLength(50)]
    public string DeptCode { get; set; } = string.Empty;

    [StringLength(200)]
    public string DeptName { get; set; } = string.Empty;
}
