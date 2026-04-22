using ConnectDB.Data;
using ConnectDB.DTOs;
using ConnectDB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConnectDB.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MembershipPackagesController : ControllerBase
{
    private readonly AppDbContext _context;

    public MembershipPackagesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MembershipPackageResponseDto>>> GetMembershipPackages()
    {
        var items = await _context.MembershipPackages
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => MapToResponse(p))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MembershipPackageResponseDto>> GetMembershipPackage(int id)
    {
        var package = await _context.MembershipPackages.FindAsync(id);
        if (package == null)
        {
            return NotFound();
        }

        return Ok(MapToResponse(package));
    }

    [HttpPost]
    public async Task<ActionResult<MembershipPackageResponseDto>> CreateMembershipPackage(MembershipPackageCreateDto dto)
    {
        var code = dto.PackageCode.Trim();
        var duplicate = await _context.MembershipPackages.AnyAsync(p => p.PackageCode.ToLower() == code.ToLower());
        if (duplicate)
        {
            return BadRequest(new { message = "PackageCode already exists." });
        }

        var package = new MembershipPackage
        {
            PackageCode = code,
            Name = dto.Name.Trim(),
            Description = dto.Description,
            DurationDays = dto.DurationDays,
            Price = dto.Price,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        _context.MembershipPackages.Add(package);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMembershipPackage), new { id = package.Id }, MapToResponse(package));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMembershipPackage(int id, MembershipPackageUpdateDto dto)
    {
        var package = await _context.MembershipPackages.FindAsync(id);
        if (package == null)
        {
            return NotFound();
        }

        var code = dto.PackageCode.Trim();
        var duplicate = await _context.MembershipPackages.AnyAsync(p =>
            p.Id != id && p.PackageCode.ToLower() == code.ToLower());
        if (duplicate)
        {
            return BadRequest(new { message = "PackageCode already exists." });
        }

        package.PackageCode = code;
        package.Name = dto.Name.Trim();
        package.Description = dto.Description;
        package.DurationDays = dto.DurationDays;
        package.Price = dto.Price;
        package.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdatePackageStatus(int id, MembershipPackageStatusUpdateDto dto)
    {
        var package = await _context.MembershipPackages.FindAsync(id);
        if (package == null)
        {
            return NotFound();
        }

        package.IsActive = dto.IsActive;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMembershipPackage(int id)
    {
        var package = await _context.MembershipPackages.FindAsync(id);
        if (package == null)
        {
            return NotFound();
        }

        var isUsed = await _context.Subscriptions.AnyAsync(s => s.MembershipPackageId == id);
        if (isUsed)
        {
            return BadRequest(new { message = "Cannot delete package because it has subscriptions." });
        }

        _context.MembershipPackages.Remove(package);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static MembershipPackageResponseDto MapToResponse(MembershipPackage package)
    {
        return new MembershipPackageResponseDto
        {
            Id = package.Id,
            PackageCode = package.PackageCode,
            Name = package.Name,
            Description = package.Description,
            DurationDays = package.DurationDays,
            Price = package.Price,
            IsActive = package.IsActive,
            CreatedAt = package.CreatedAt
        };
    }
}
