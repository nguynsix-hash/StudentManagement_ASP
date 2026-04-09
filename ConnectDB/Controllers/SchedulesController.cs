using ConnectDB.Data;
using ConnectDB.DTOs;
using ConnectDB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConnectDB.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SchedulesController : ControllerBase
{
    private readonly AppDbContext _context;

    public SchedulesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScheduleResponseDto>>> GetSchedules()
    {
        var schedules = await _context.Schedules
            .Include(s => s.Trainer)
            .Include(s => s.Member)
            .OrderByDescending(s => s.ScheduleDate)
            .ThenBy(s => s.StartTime)
            .Select(s => MapToResponse(s))
            .ToListAsync();

        return Ok(schedules);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ScheduleResponseDto>> GetSchedule(int id)
    {
        var schedule = await _context.Schedules
            .Include(s => s.Trainer)
            .Include(s => s.Member)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (schedule == null)
        {
            return NotFound();
        }

        return Ok(MapToResponse(schedule));
    }

    [HttpGet("trainer/{trainerId}")]
    public async Task<ActionResult<IEnumerable<ScheduleResponseDto>>> GetSchedulesByTrainer(int trainerId)
    {
        var trainerExists = await _context.Trainers.AnyAsync(t => t.Id == trainerId);
        if (!trainerExists)
        {
            return NotFound(new { message = "Trainer not found." });
        }

        var schedules = await _context.Schedules
            .Include(s => s.Trainer)
            .Include(s => s.Member)
            .Where(s => s.TrainerId == trainerId)
            .OrderByDescending(s => s.ScheduleDate)
            .ThenBy(s => s.StartTime)
            .Select(s => MapToResponse(s))
            .ToListAsync();

        return Ok(schedules);
    }

    [HttpGet("member/{memberId}")]
    public async Task<ActionResult<IEnumerable<ScheduleResponseDto>>> GetSchedulesByMember(int memberId)
    {
        var memberExists = await _context.Members.AnyAsync(m => m.Id == memberId);
        if (!memberExists)
        {
            return NotFound(new { message = "Member not found." });
        }

        var schedules = await _context.Schedules
            .Include(s => s.Trainer)
            .Include(s => s.Member)
            .Where(s => s.MemberId == memberId)
            .OrderByDescending(s => s.ScheduleDate)
            .ThenBy(s => s.StartTime)
            .Select(s => MapToResponse(s))
            .ToListAsync();

        return Ok(schedules);
    }

    [HttpPost]
    public async Task<ActionResult<ScheduleResponseDto>> CreateSchedule(ScheduleCreateDto dto)
    {
        if (dto.StartTime >= dto.EndTime)
        {
            return BadRequest(new { message = "StartTime must be earlier than EndTime." });
        }

        var trainer = await _context.Trainers.FindAsync(dto.TrainerId);
        if (trainer == null)
        {
            return BadRequest(new { message = "TrainerId does not exist." });
        }

        if (!trainer.IsActive)
        {
            return BadRequest(new { message = "Trainer is inactive." });
        }

        if (dto.MemberId.HasValue)
        {
            var member = await _context.Members.FindAsync(dto.MemberId.Value);
            if (member == null)
            {
                return BadRequest(new { message = "MemberId does not exist." });
            }

            if (!member.IsActive)
            {
                return BadRequest(new { message = "Member is inactive." });
            }
        }

        var hasTrainerOverlap = await HasTrainerOverlapAsync(
            dto.TrainerId,
            dto.ScheduleDate.Date,
            dto.StartTime,
            dto.EndTime);
        if (hasTrainerOverlap)
        {
            return BadRequest(new { message = "Trainer has overlapping schedule." });
        }

        if (dto.MemberId.HasValue)
        {
            var hasMemberOverlap = await HasMemberOverlapAsync(
                dto.MemberId.Value,
                dto.ScheduleDate.Date,
                dto.StartTime,
                dto.EndTime);
            if (hasMemberOverlap)
            {
                return BadRequest(new { message = "Member has overlapping schedule." });
            }
        }

        var schedule = new Schedule
        {
            Title = dto.Title.Trim(),
            ScheduleDate = dto.ScheduleDate.Date,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            TrainerId = dto.TrainerId,
            MemberId = dto.MemberId,
            Notes = dto.Notes
        };

        _context.Schedules.Add(schedule);
        await _context.SaveChangesAsync();

        var created = await _context.Schedules
            .Include(s => s.Trainer)
            .Include(s => s.Member)
            .FirstAsync(s => s.Id == schedule.Id);

        return CreatedAtAction(nameof(GetSchedule), new { id = schedule.Id }, MapToResponse(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSchedule(int id, ScheduleUpdateDto dto)
    {
        if (dto.StartTime >= dto.EndTime)
        {
            return BadRequest(new { message = "StartTime must be earlier than EndTime." });
        }

        var schedule = await _context.Schedules.FindAsync(id);
        if (schedule == null)
        {
            return NotFound();
        }

        var trainer = await _context.Trainers.FindAsync(dto.TrainerId);
        if (trainer == null)
        {
            return BadRequest(new { message = "TrainerId does not exist." });
        }

        if (dto.MemberId.HasValue)
        {
            var member = await _context.Members.FindAsync(dto.MemberId.Value);
            if (member == null)
            {
                return BadRequest(new { message = "MemberId does not exist." });
            }
        }

        var hasTrainerOverlap = await HasTrainerOverlapAsync(
            dto.TrainerId,
            dto.ScheduleDate.Date,
            dto.StartTime,
            dto.EndTime,
            id);
        if (hasTrainerOverlap)
        {
            return BadRequest(new { message = "Trainer has overlapping schedule." });
        }

        if (dto.MemberId.HasValue)
        {
            var hasMemberOverlap = await HasMemberOverlapAsync(
                dto.MemberId.Value,
                dto.ScheduleDate.Date,
                dto.StartTime,
                dto.EndTime,
                id);
            if (hasMemberOverlap)
            {
                return BadRequest(new { message = "Member has overlapping schedule." });
            }
        }

        schedule.Title = dto.Title.Trim();
        schedule.ScheduleDate = dto.ScheduleDate.Date;
        schedule.StartTime = dto.StartTime;
        schedule.EndTime = dto.EndTime;
        schedule.TrainerId = dto.TrainerId;
        schedule.MemberId = dto.MemberId;
        schedule.Notes = dto.Notes;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSchedule(int id)
    {
        var schedule = await _context.Schedules.FindAsync(id);
        if (schedule == null)
        {
            return NotFound();
        }

        _context.Schedules.Remove(schedule);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private async Task<bool> HasTrainerOverlapAsync(
        int trainerId,
        DateTime scheduleDate,
        TimeSpan startTime,
        TimeSpan endTime,
        int? excludedScheduleId = null)
    {
        return await _context.Schedules.AnyAsync(s =>
            s.TrainerId == trainerId &&
            s.ScheduleDate == scheduleDate &&
            (!excludedScheduleId.HasValue || s.Id != excludedScheduleId.Value) &&
            s.StartTime < endTime &&
            startTime < s.EndTime);
    }

    private async Task<bool> HasMemberOverlapAsync(
        int memberId,
        DateTime scheduleDate,
        TimeSpan startTime,
        TimeSpan endTime,
        int? excludedScheduleId = null)
    {
        return await _context.Schedules.AnyAsync(s =>
            s.MemberId == memberId &&
            s.ScheduleDate == scheduleDate &&
            (!excludedScheduleId.HasValue || s.Id != excludedScheduleId.Value) &&
            s.StartTime < endTime &&
            startTime < s.EndTime);
    }

    private static ScheduleResponseDto MapToResponse(Schedule schedule)
    {
        return new ScheduleResponseDto
        {
            Id = schedule.Id,
            Title = schedule.Title,
            ScheduleDate = schedule.ScheduleDate,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            TrainerId = schedule.TrainerId,
            TrainerName = schedule.Trainer?.FullName ?? string.Empty,
            MemberId = schedule.MemberId,
            MemberName = schedule.Member?.FullName,
            Notes = schedule.Notes
        };
    }
}
