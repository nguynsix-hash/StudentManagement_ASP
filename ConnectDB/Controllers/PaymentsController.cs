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
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PaymentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentResponseDto>>> GetPayments()
    {
        var items = await _context.Payments
            .Include(p => p.Subscription)
                .ThenInclude(s => s!.Member)
            .Include(p => p.Subscription)
                .ThenInclude(s => s!.MembershipPackage)
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => MapToResponse(p))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentResponseDto>> GetPayment(int id)
    {
        var payment = await _context.Payments
            .Include(p => p.Subscription)
                .ThenInclude(s => s!.Member)
            .Include(p => p.Subscription)
                .ThenInclude(s => s!.MembershipPackage)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payment == null)
        {
            return NotFound();
        }

        return Ok(MapToResponse(payment));
    }

    [HttpGet("subscription/{subscriptionId}")]
    public async Task<ActionResult<IEnumerable<PaymentResponseDto>>> GetPaymentsBySubscription(int subscriptionId)
    {
        var subscriptionExists = await _context.Subscriptions.AnyAsync(s => s.Id == subscriptionId);
        if (!subscriptionExists)
        {
            return NotFound(new { message = "Subscription not found." });
        }

        var items = await _context.Payments
            .Include(p => p.Subscription)
                .ThenInclude(s => s!.Member)
            .Include(p => p.Subscription)
                .ThenInclude(s => s!.MembershipPackage)
            .Where(p => p.SubscriptionId == subscriptionId)
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => MapToResponse(p))
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<PaymentResponseDto>> CreatePayment(PaymentCreateDto dto)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.Member)
            .Include(s => s.MembershipPackage)
            .FirstOrDefaultAsync(s => s.Id == dto.SubscriptionId);

        if (subscription == null)
        {
            return BadRequest(new { message = "SubscriptionId does not exist." });
        }

        var amount = dto.Amount ?? subscription.MembershipPackage?.Price ?? 0;
        if (amount <= 0)
        {
            return BadRequest(new { message = "Amount must be greater than 0." });
        }

        var payment = new Payment
        {
            SubscriptionId = dto.SubscriptionId,
            Amount = amount,
            PaymentDate = dto.PaymentDate?.Date ?? DateTime.Now,
            PaymentMethod = dto.PaymentMethod.Trim(),
            Status = dto.Status.Trim(),
            Note = dto.Note
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        var created = await _context.Payments
            .Include(p => p.Subscription)
                .ThenInclude(s => s!.Member)
            .Include(p => p.Subscription)
                .ThenInclude(s => s!.MembershipPackage)
            .FirstAsync(p => p.Id == payment.Id);

        return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, MapToResponse(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePayment(int id, PaymentUpdateDto dto)
    {
        if (dto.Amount <= 0)
        {
            return BadRequest(new { message = "Amount must be greater than 0." });
        }

        var payment = await _context.Payments.FindAsync(id);
        if (payment == null)
        {
            return NotFound();
        }

        payment.Amount = dto.Amount;
        payment.PaymentDate = dto.PaymentDate;
        payment.PaymentMethod = dto.PaymentMethod.Trim();
        payment.Status = dto.Status.Trim();
        payment.Note = dto.Note;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePayment(int id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null)
        {
            return NotFound();
        }

        _context.Payments.Remove(payment);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static PaymentResponseDto MapToResponse(Payment payment)
    {
        return new PaymentResponseDto
        {
            Id = payment.Id,
            SubscriptionId = payment.SubscriptionId,
            MemberId = payment.Subscription?.MemberId ?? 0,
            MemberCode = payment.Subscription?.Member?.MemberCode ?? string.Empty,
            MemberName = payment.Subscription?.Member?.FullName ?? string.Empty,
            MembershipPackageId = payment.Subscription?.MembershipPackageId ?? 0,
            PackageName = payment.Subscription?.MembershipPackage?.Name ?? string.Empty,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            PaymentMethod = payment.PaymentMethod,
            Status = payment.Status,
            Note = payment.Note
        };
    }
}
