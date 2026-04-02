using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConnectDB.Data;
using ConnectDB.Models;
using ConnectDB.DTOs;

namespace ConnectDB.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DepartmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public DepartmentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments()
    {
        var departments = await _context.Departments
            .Select(d => new DepartmentDto
            {
                Id = d.Id,
                DeptCode = d.DeptCode,
                DeptName = d.DeptName
            })
            .ToListAsync();

        return Ok(departments);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DepartmentDto>> GetDepartment(int id)
    {
        var department = await _context.Departments.FindAsync(id);

        if (department == null)
            return NotFound();

        return Ok(new DepartmentDto
        {
            Id = department.Id,
            DeptCode = department.DeptCode,
            DeptName = department.DeptName
        });
    }

    [HttpPost]
    public async Task<ActionResult<DepartmentDto>> CreateDepartment(CreateDepartmentDto createDto)
    {
        var department = new Department
        {
            DeptCode = createDto.DeptCode,
            DeptName = createDto.DeptName
        };

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        var dto = new DepartmentDto
        {
            Id = department.Id,
            DeptCode = department.DeptCode,
            DeptName = department.DeptName
        };

        return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDepartment(int id, UpdateDepartmentDto updateDto)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null)
            return NotFound();

        department.DeptCode = updateDto.DeptCode;
        department.DeptName = updateDto.DeptName;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DepartmentExists(id))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null)
            return NotFound();

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool DepartmentExists(int id)
    {
        return _context.Departments.Any(e => e.Id == id);
    }
}
