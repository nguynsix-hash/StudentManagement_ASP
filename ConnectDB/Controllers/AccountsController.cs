using ConnectDB.Data;
using ConnectDB.DTOs;
using ConnectDB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ConnectDB.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AccountsController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountResponseDto>>> GetAccounts()
    {
        var items = await _context.Accounts
            .Include(a => a.Role)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => MapToResponse(a))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AccountResponseDto>> GetAccount(int id)
    {
        var account = await _context.Accounts
            .Include(a => a.Role)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (account == null)
        {
            return NotFound();
        }

        return Ok(MapToResponse(account));
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AccountResponseDto>> Register(AccountRegisterDto dto)
    {
        var username = dto.Username.Trim();
        var exists = await _context.Accounts.AnyAsync(a => a.Username.ToLower() == username.ToLower());
        if (exists)
        {
            return BadRequest(new { message = "Username already exists." });
        }

        var roleId = dto.RoleId;
        if (!roleId.HasValue)
        {
            roleId = await _context.Roles
                .Where(r => r.Name == "Member")
                .Select(r => (int?)r.Id)
                .FirstOrDefaultAsync() ?? 4;
        }

        var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId.Value);
        if (!roleExists)
        {
            return BadRequest(new { message = "RoleId does not exist." });
        }

        var account = new Account
        {
            Username = username,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FullName = dto.FullName.Trim(),
            Email = dto.Email,
            Phone = dto.Phone,
            RoleId = roleId.Value,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var created = await _context.Accounts
            .Include(a => a.Role)
            .FirstAsync(a => a.Id == account.Id);

        return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, MapToResponse(created));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AccountLoginResponseDto>> Login(AccountLoginDto dto)
    {
        var username = dto.Username.Trim();
        var account = await _context.Accounts
            .Include(a => a.Role)
            .FirstOrDefaultAsync(a =>
                a.Username.ToLower() == username.ToLower() &&
                a.IsActive);

        if (account == null || !VerifyPassword(account.Password, dto.Password))
        {
            return Unauthorized(new { message = "Invalid credentials or inactive account." });
        }

        if (!IsBcryptHash(account.Password))
        {
            account.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            await _context.SaveChangesAsync();
        }

        var accountResponse = MapToResponse(account);
        var expiresAt = DateTime.UtcNow.AddHours(8);
        var token = GenerateJwtToken(accountResponse, expiresAt);

        return Ok(new AccountLoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            Account = accountResponse
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccount(int id, AccountUpdateDto dto)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account == null)
        {
            return NotFound();
        }

        var roleExists = await _context.Roles.AnyAsync(r => r.Id == dto.RoleId);
        if (!roleExists)
        {
            return BadRequest(new { message = "RoleId does not exist." });
        }

        account.FullName = dto.FullName.Trim();
        account.Email = dto.Email;
        account.Phone = dto.Phone;
        account.RoleId = dto.RoleId;
        account.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account == null)
        {
            return NotFound();
        }

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}/password")]
    public async Task<IActionResult> ChangePassword(int id, AccountChangePasswordDto dto)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account == null)
        {
            return NotFound();
        }

        if (!VerifyPassword(account.Password, dto.CurrentPassword))
        {
            return BadRequest(new { message = "Current password is incorrect." });
        }

        account.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static AccountResponseDto MapToResponse(Account account)
    {
        return new AccountResponseDto
        {
            Id = account.Id,
            Username = account.Username,
            FullName = account.FullName,
            Email = account.Email,
            Phone = account.Phone,
            RoleId = account.RoleId,
            RoleName = account.Role?.Name ?? string.Empty,
            IsActive = account.IsActive,
            CreatedAt = account.CreatedAt
        };
    }

    private static bool VerifyPassword(string storedPassword, string providedPassword)
    {
        return IsBcryptHash(storedPassword)
            ? BCrypt.Net.BCrypt.Verify(providedPassword, storedPassword)
            : storedPassword == providedPassword;
    }

    private static bool IsBcryptHash(string password)
    {
        return password.StartsWith("$2a$") || password.StartsWith("$2b$") || password.StartsWith("$2y$");
    }

    private string GenerateJwtToken(AccountResponseDto account, DateTime expiresAtUtc)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var key = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var issuer = jwtSection["Issuer"] ?? "GymManagementApi";
        var audience = jwtSection["Audience"] ?? "GymManagementAdmin";

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, account.Username),
            new(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new(ClaimTypes.Name, account.Username),
            new(ClaimTypes.Role, account.RoleName),
            new("fullName", account.FullName),
            new("roleId", account.RoleId.ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
