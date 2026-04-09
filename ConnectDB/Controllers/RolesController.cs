using ConnectDB.Data;
using ConnectDB.DTOs;
using ConnectDB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConnectDB.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _context;

    public RolesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleResponseDto>>> GetRoles()
    {
        var roles = await _context.Roles
            .OrderBy(r => r.Name)
            .Select(r => new RoleResponseDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description
            })
            .ToListAsync();

        return Ok(roles);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoleResponseDto>> GetRole(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        return Ok(new RoleResponseDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description
        });
    }

    [HttpPost]
    public async Task<ActionResult<RoleResponseDto>> CreateRole(RoleCreateDto dto)
    {
        var exists = await _context.Roles.AnyAsync(r => r.Name.ToLower() == dto.Name.ToLower());
        if (exists)
        {
            return BadRequest(new { message = "Role name already exists." });
        }

        var role = new Role
        {
            Name = dto.Name.Trim(),
            Description = dto.Description
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        var response = new RoleResponseDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description
        };

        return CreatedAtAction(nameof(GetRole), new { id = role.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(int id, RoleUpdateDto dto)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        var duplicate = await _context.Roles.AnyAsync(r =>
            r.Id != id && r.Name.ToLower() == dto.Name.ToLower());
        if (duplicate)
        {
            return BadRequest(new { message = "Role name already exists." });
        }

        role.Name = dto.Name.Trim();
        role.Description = dto.Description;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        var inUse = await _context.Accounts.AnyAsync(a => a.RoleId == id);
        if (inUse)
        {
            return BadRequest(new { message = "Cannot delete role because it is in use by accounts." });
        }

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
