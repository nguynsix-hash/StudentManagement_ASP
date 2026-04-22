using ConnectDB.Data;
using ConnectDB.DTOs;
using ConnectDB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConnectDB.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AttendancesController : ControllerBase
{
    private readonly AppDbContext _context;

    public AttendancesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AttendanceResponseDto>>> GetAttendances(
        [FromQuery] int? scheduleId,
        [FromQuery] int? memberId)
    {
        var query = _context.Attendances
            .Include(a => a.Schedule)
            .Include(a => a.Member)
            .AsQueryable();

        if (scheduleId.HasValue)
        {
            query = query.Where(a => a.ScheduleId == scheduleId.Value);
        }

        if (memberId.HasValue)
        {
            query = query.Where(a => a.MemberId == memberId.Value);
        }

        var items = await query
            .OrderByDescending(a => a.RecordedAt)
            .Select(a => MapToResponse(a))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AttendanceResponseDto>> GetAttendance(int id)
    {
        var attendance = await _context.Attendances
            .Include(a => a.Schedule)
            .Include(a => a.Member)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (attendance == null)
        {
            return NotFound();
        }

        return Ok(MapToResponse(attendance));
    }

    [HttpGet("schedule/{scheduleId}")]
    public async Task<ActionResult<IEnumerable<AttendanceResponseDto>>> GetAttendanceBySchedule(int scheduleId)
    {
        var scheduleExists = await _context.Schedules.AnyAsync(s => s.Id == scheduleId);
        if (!scheduleExists)
        {
            return NotFound(new { message = "Schedule not found." });
        }

        var items = await _context.Attendances
            .Include(a => a.Schedule)
            .Include(a => a.Member)
            .Where(a => a.ScheduleId == scheduleId)
            .OrderByDescending(a => a.RecordedAt)
            .Select(a => MapToResponse(a))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("member/{memberId}/history")]
    public async Task<ActionResult<IEnumerable<AttendanceResponseDto>>> GetAttendanceHistoryByMember(int memberId)
    {
        var memberExists = await _context.Members.AnyAsync(m => m.Id == memberId);
        if (!memberExists)
        {
            return NotFound(new { message = "Member not found." });
        }

        var items = await _context.Attendances
            .Include(a => a.Schedule)
            .Include(a => a.Member)
            .Where(a => a.MemberId == memberId)
            .OrderByDescending(a => a.Schedule!.ScheduleDate)
            .ThenByDescending(a => a.Schedule!.StartTime)
            .Select(a => MapToResponse(a))
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("mark")]
    public async Task<ActionResult<AttendanceResponseDto>> MarkAttendance(AttendanceMarkDto dto)
    {
        var schedule = await _context.Schedules.FindAsync(dto.ScheduleId);
        if (schedule == null)
        {
            return BadRequest(new { message = "ScheduleId does not exist." });
        }

        var member = await _context.Members.FindAsync(dto.MemberId);
        if (member == null)
        {
            return BadRequest(new { message = "MemberId does not exist." });
        }

        if (schedule.MemberId.HasValue && schedule.MemberId.Value != dto.MemberId)
        {
            return BadRequest(new { message = "Member is not assigned to this schedule." });
        }

        var normalizedStatus = dto.Status.Trim();
        if (!IsValidStatus(normalizedStatus))
        {
            return BadRequest(new { message = "Status must be Present, Absent or Late." });
        }

        var existing = await _context.Attendances
            .Include(a => a.Schedule)
            .Include(a => a.Member)
            .FirstOrDefaultAsync(a => a.ScheduleId == dto.ScheduleId && a.MemberId == dto.MemberId);

        if (existing != null)
        {
            existing.Status = normalizedStatus;
            existing.Note = dto.Note;
            existing.RecordedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return Ok(MapToResponse(existing));
        }

        var attendance = new Attendance
        {
            ScheduleId = dto.ScheduleId,
            MemberId = dto.MemberId,
            Status = normalizedStatus,
            Note = dto.Note,
            RecordedAt = DateTime.Now
        };

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();

        var created = await _context.Attendances
            .Include(a => a.Schedule)
            .Include(a => a.Member)
            .FirstAsync(a => a.Id == attendance.Id);

        return CreatedAtAction(nameof(GetAttendance), new { id = attendance.Id }, MapToResponse(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAttendance(int id, AttendanceUpdateDto dto)
    {
        var attendance = await _context.Attendances.FindAsync(id);
        if (attendance == null)
        {
            return NotFound();
        }

        var normalizedStatus = dto.Status.Trim();
        if (!IsValidStatus(normalizedStatus))
        {
            return BadRequest(new { message = "Status must be Present, Absent or Late." });
        }

        attendance.Status = normalizedStatus;
        attendance.Note = dto.Note;
        attendance.RecordedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAttendance(int id)
    {
        var attendance = await _context.Attendances.FindAsync(id);
        if (attendance == null)
        {
            return NotFound();
        }

        _context.Attendances.Remove(attendance);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static bool IsValidStatus(string status)
    {
        return status == "Present" || status == "Absent" || status == "Late";
    }

    private static AttendanceResponseDto MapToResponse(Attendance attendance)
    {
        return new AttendanceResponseDto
        {
            Id = attendance.Id,
            ScheduleId = attendance.ScheduleId,
            ScheduleTitle = attendance.Schedule?.Title ?? string.Empty,
            ScheduleDate = attendance.Schedule?.ScheduleDate ?? default,
            MemberId = attendance.MemberId,
            MemberCode = attendance.Member?.MemberCode ?? string.Empty,
            MemberName = attendance.Member?.FullName ?? string.Empty,
            Status = attendance.Status,
            Note = attendance.Note,
            RecordedAt = attendance.RecordedAt
        };
    }
}
