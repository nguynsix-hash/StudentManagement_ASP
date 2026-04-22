using ConnectDB.Data;
using ConnectDB.DTOs;
using ConnectDB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConnectDB.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MembersController : ControllerBase
{
    private readonly AppDbContext _context;

    public MembersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberResponseDto>>> GetMembers()
    {
        var members = await _context.Members
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => MapToResponse(m))
            .ToListAsync();

        return Ok(members);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MemberResponseDto>> GetMember(int id)
    {
        var member = await _context.Members.FindAsync(id);
        if (member == null)
        {
            return NotFound();
        }

        return Ok(MapToResponse(member));
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<MemberResponseDto>>> SearchMembers([FromQuery] string keyword)
    {
        var normalized = keyword?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return Ok(new List<MemberResponseDto>());
        }

        var members = await _context.Members
            .Where(m =>
                m.MemberCode.Contains(normalized) ||
                m.FullName.Contains(normalized) ||
                (m.Phone != null && m.Phone.Contains(normalized)))
            .OrderBy(m => m.FullName)
            .Select(m => MapToResponse(m))
            .ToListAsync();

        return Ok(members);
    }

    [HttpPost]
    public async Task<ActionResult<MemberResponseDto>> CreateMember(MemberCreateDto dto)
    {
        var code = dto.MemberCode.Trim();
        var duplicate = await _context.Members.AnyAsync(m => m.MemberCode.ToLower() == code.ToLower());
        if (duplicate)
        {
            return BadRequest(new { message = "MemberCode already exists." });
        }

        var member = new Member
        {
            MemberCode = code,
            FullName = dto.FullName.Trim(),
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMember), new { id = member.Id }, MapToResponse(member));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMember(int id, MemberUpdateDto dto)
    {
        var member = await _context.Members.FindAsync(id);
        if (member == null)
        {
            return NotFound();
        }

        var code = dto.MemberCode.Trim();
        var duplicate = await _context.Members.AnyAsync(m =>
            m.Id != id && m.MemberCode.ToLower() == code.ToLower());
        if (duplicate)
        {
            return BadRequest(new { message = "MemberCode already exists." });
        }

        member.MemberCode = code;
        member.FullName = dto.FullName.Trim();
        member.DateOfBirth = dto.DateOfBirth;
        member.Gender = dto.Gender;
        member.Phone = dto.Phone;
        member.Email = dto.Email;
        member.Address = dto.Address;
        member.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateMemberStatus(int id, MemberStatusUpdateDto dto)
    {
        var member = await _context.Members.FindAsync(id);
        if (member == null)
        {
            return NotFound();
        }

        member.IsActive = dto.IsActive;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMember(int id)
    {
        var member = await _context.Members.FindAsync(id);
        if (member == null)
        {
            return NotFound();
        }

        var hasSubscriptions = await _context.Subscriptions.AnyAsync(s => s.MemberId == id);
        if (hasSubscriptions)
        {
            return BadRequest(new { message = "Cannot delete member because it has subscriptions." });
        }

        var hasAttendances = await _context.Attendances.AnyAsync(a => a.MemberId == id);
        if (hasAttendances)
        {
            return BadRequest(new { message = "Cannot delete member because it has attendance records." });
        }

        var hasSchedules = await _context.Schedules.AnyAsync(s => s.MemberId == id);
        if (hasSchedules)
        {
            return BadRequest(new { message = "Cannot delete member because it is assigned to schedules." });
        }

        _context.Members.Remove(member);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static MemberResponseDto MapToResponse(Member member)
    {
        return new MemberResponseDto
        {
            Id = member.Id,
            MemberCode = member.MemberCode,
            FullName = member.FullName,
            DateOfBirth = member.DateOfBirth,
            Gender = member.Gender,
            Phone = member.Phone,
            Email = member.Email,
            Address = member.Address,
            IsActive = member.IsActive,
            CreatedAt = member.CreatedAt
        };
    }
}
