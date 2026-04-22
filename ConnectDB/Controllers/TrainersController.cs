using ConnectDB.Data;
using ConnectDB.DTOs;
using ConnectDB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConnectDB.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TrainersController : ControllerBase
{
    private readonly AppDbContext _context;

    public TrainersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrainerResponseDto>>> GetTrainers()
    {
        var trainers = await _context.Trainers
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => MapToResponse(t))
            .ToListAsync();

        return Ok(trainers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TrainerResponseDto>> GetTrainer(int id)
    {
        var trainer = await _context.Trainers.FindAsync(id);
        if (trainer == null)
        {
            return NotFound();
        }

        return Ok(MapToResponse(trainer));
    }

    [HttpPost]
    public async Task<ActionResult<TrainerResponseDto>> CreateTrainer(TrainerCreateDto dto)
    {
        var code = dto.TrainerCode.Trim();
        var duplicate = await _context.Trainers.AnyAsync(t => t.TrainerCode.ToLower() == code.ToLower());
        if (duplicate)
        {
            return BadRequest(new { message = "TrainerCode already exists." });
        }

        var trainer = new Trainer
        {
            TrainerCode = code,
            FullName = dto.FullName.Trim(),
            Specialty = dto.Specialty,
            Phone = dto.Phone,
            Email = dto.Email,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        _context.Trainers.Add(trainer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTrainer), new { id = trainer.Id }, MapToResponse(trainer));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTrainer(int id, TrainerUpdateDto dto)
    {
        var trainer = await _context.Trainers.FindAsync(id);
        if (trainer == null)
        {
            return NotFound();
        }

        var code = dto.TrainerCode.Trim();
        var duplicate = await _context.Trainers.AnyAsync(t =>
            t.Id != id && t.TrainerCode.ToLower() == code.ToLower());
        if (duplicate)
        {
            return BadRequest(new { message = "TrainerCode already exists." });
        }

        trainer.TrainerCode = code;
        trainer.FullName = dto.FullName.Trim();
        trainer.Specialty = dto.Specialty;
        trainer.Phone = dto.Phone;
        trainer.Email = dto.Email;
        trainer.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateTrainerStatus(int id, TrainerStatusUpdateDto dto)
    {
        var trainer = await _context.Trainers.FindAsync(id);
        if (trainer == null)
        {
            return NotFound();
        }

        trainer.IsActive = dto.IsActive;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTrainer(int id)
    {
        var trainer = await _context.Trainers.FindAsync(id);
        if (trainer == null)
        {
            return NotFound();
        }

        var hasSchedules = await _context.Schedules.AnyAsync(s => s.TrainerId == id);
        if (hasSchedules)
        {
            return BadRequest(new { message = "Cannot delete trainer because it is assigned to schedules." });
        }

        _context.Trainers.Remove(trainer);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static TrainerResponseDto MapToResponse(Trainer trainer)
    {
        return new TrainerResponseDto
        {
            Id = trainer.Id,
            TrainerCode = trainer.TrainerCode,
            FullName = trainer.FullName,
            Specialty = trainer.Specialty,
            Phone = trainer.Phone,
            Email = trainer.Email,
            IsActive = trainer.IsActive,
            CreatedAt = trainer.CreatedAt
        };
    }
}
