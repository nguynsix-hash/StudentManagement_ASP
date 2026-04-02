using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConnectDB.Data;
using ConnectDB.Models;
using ConnectDB.DTOs;

namespace ConnectDB.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VendorsController : ControllerBase
{
    private readonly AppDbContext _context;

    public VendorsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VendorDto>>> GetVendors()
    {
        var vendors = await _context.Vendors
            .Select(v => new VendorDto
            {
                Id = v.Id,
                VendorCode = v.VendorCode,
                VendorName = v.VendorName,
                TaxNo = v.TaxNo,
                PaymentTerms = v.PaymentTerms
            })
            .ToListAsync();

        return Ok(vendors);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VendorDto>> GetVendor(int id)
    {
        var vendor = await _context.Vendors.FindAsync(id);

        if (vendor == null)
            return NotFound();

        return Ok(new VendorDto
        {
            Id = vendor.Id,
            VendorCode = vendor.VendorCode,
            VendorName = vendor.VendorName,
            TaxNo = vendor.TaxNo,
            PaymentTerms = vendor.PaymentTerms
        });
    }

    [HttpPost]
    public async Task<ActionResult<VendorDto>> CreateVendor(CreateVendorDto createDto)
    {
        var vendor = new Vendor
        {
            VendorCode = createDto.VendorCode,
            VendorName = createDto.VendorName,
            TaxNo = createDto.TaxNo,
            PaymentTerms = createDto.PaymentTerms
        };

        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();

        var dto = new VendorDto
        {
            Id = vendor.Id,
            VendorCode = vendor.VendorCode,
            VendorName = vendor.VendorName,
            TaxNo = vendor.TaxNo,
            PaymentTerms = vendor.PaymentTerms
        };

        return CreatedAtAction(nameof(GetVendor), new { id = vendor.Id }, dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVendor(int id, UpdateVendorDto updateDto)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null)
            return NotFound();

        vendor.VendorCode = updateDto.VendorCode;
        vendor.VendorName = updateDto.VendorName;
        vendor.TaxNo = updateDto.TaxNo;
        vendor.PaymentTerms = updateDto.PaymentTerms;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!VendorExists(id))
                return NotFound();
            else
                throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVendor(int id)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null)
            return NotFound();

        _context.Vendors.Remove(vendor);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool VendorExists(int id)
    {
        return _context.Vendors.Any(e => e.Id == id);
    }
}
