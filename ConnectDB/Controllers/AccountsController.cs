using ConnectDB.Data;
using ConnectDB.DTOs;
using ConnectDB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConnectDB.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AccountsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountResponseDto>>> GetAccounts()
    {
        var accounts = await _context.Accounts
            .Include(a => a.Role)
            .Select(a => MapToResponse(a))
            .ToListAsync();

        return Ok(accounts);
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
            Password = dto.Password,
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
    public async Task<ActionResult<AccountResponseDto>> Login(AccountLoginDto dto)
    {
        var username = dto.Username.Trim();
        var account = await _context.Accounts
            .Include(a => a.Role)
            .FirstOrDefaultAsync(a =>
                a.Username.ToLower() == username.ToLower() &&
                a.Password == dto.Password &&
                a.IsActive);

        if (account == null)
        {
            return Unauthorized(new { message = "Invalid credentials or inactive account." });
        }

        return Ok(MapToResponse(account));
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
}
