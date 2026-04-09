using System.ComponentModel.DataAnnotations;

namespace ConnectDB.DTOs;

public class AccountRegisterDto
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(15)]
    public string? Phone { get; set; }

    public int? RoleId { get; set; }
}

public class AccountLoginDto
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string Password { get; set; } = string.Empty;
}

public class AccountUpdateDto
{
    [Required]
    [StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(15)]
    public string? Phone { get; set; }

    public int RoleId { get; set; }

    public bool IsActive { get; set; }
}

public class AccountResponseDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
