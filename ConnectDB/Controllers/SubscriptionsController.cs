using ConnectDB.Data;
using ConnectDB.DTOs;
using ConnectDB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConnectDB.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SubscriptionsController : ControllerBase
{
    private readonly AppDbContext _context;

    public SubscriptionsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SubscriptionResponseDto>>> GetSubscriptions()
    {
        var subscriptions = await _context.Subscriptions
            .Include(s => s.Member)
            .Include(s => s.MembershipPackage)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => MapToResponse(s))
            .ToListAsync();

        return Ok(subscriptions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SubscriptionResponseDto>> GetSubscription(int id)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.Member)
            .Include(s => s.MembershipPackage)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (subscription == null)
        {
            return NotFound();
        }

        return Ok(MapToResponse(subscription));
    }

    [HttpGet("member/{memberId}")]
    public async Task<ActionResult<IEnumerable<SubscriptionResponseDto>>> GetSubscriptionsByMember(int memberId)
    {
        var memberExists = await _context.Members.AnyAsync(m => m.Id == memberId);
        if (!memberExists)
        {
            return NotFound(new { message = "Member not found." });
        }

        var subscriptions = await _context.Subscriptions
            .Include(s => s.Member)
            .Include(s => s.MembershipPackage)
            .Where(s => s.MemberId == memberId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => MapToResponse(s))
            .ToListAsync();

        return Ok(subscriptions);
    }

    [HttpPost]
    public async Task<ActionResult<SubscriptionResponseDto>> CreateSubscription(SubscriptionCreateDto dto)
    {
        var member = await _context.Members.FindAsync(dto.MemberId);
        if (member == null)
        {
            return BadRequest(new { message = "MemberId does not exist." });
        }

        if (!member.IsActive)
        {
            return BadRequest(new { message = "Member is inactive." });
        }

        var package = await _context.MembershipPackages.FindAsync(dto.MembershipPackageId);
        if (package == null)
        {
            return BadRequest(new { message = "MembershipPackageId does not exist." });
        }

        if (!package.IsActive)
        {
            return BadRequest(new { message = "Membership package is inactive." });
        }

        var startDate = (dto.StartDate ?? DateTime.Today).Date;
        var endDate = startDate.AddDays(package.DurationDays);

        var subscription = new Subscription
        {
            MemberId = dto.MemberId,
            MembershipPackageId = dto.MembershipPackageId,
            StartDate = startDate,
            EndDate = endDate,
            Status = endDate >= DateTime.Today ? "Active" : "Expired",
            CreatedAt = DateTime.Now
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        var created = await _context.Subscriptions
            .Include(s => s.Member)
            .Include(s => s.MembershipPackage)
            .FirstAsync(s => s.Id == subscription.Id);

        return CreatedAtAction(nameof(GetSubscription), new { id = created.Id }, MapToResponse(created));
    }

    [HttpPost("{id}/extend")]
    public async Task<ActionResult<SubscriptionResponseDto>> ExtendSubscription(int id, SubscriptionExtendDto dto)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.Member)
            .Include(s => s.MembershipPackage)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (subscription == null)
        {
            return NotFound();
        }

        var extraDays = dto.ExtraDays ?? subscription.MembershipPackage?.DurationDays ?? 0;
        if (extraDays <= 0)
        {
            return BadRequest(new { message = "ExtraDays must be greater than 0." });
        }

        var baseDate = subscription.EndDate.Date >= DateTime.Today
            ? subscription.EndDate.Date
            : DateTime.Today;

        subscription.EndDate = baseDate.AddDays(extraDays);
        subscription.Status = "Active";

        await _context.SaveChangesAsync();

        return Ok(MapToResponse(subscription));
    }

    [HttpGet("{id}/status")]
    public async Task<ActionResult<SubscriptionStatusDto>> GetSubscriptionStatus(int id)
    {
        var subscription = await _context.Subscriptions.FindAsync(id);
        if (subscription == null)
        {
            return NotFound();
        }

        var currentStatus = NormalizeStatusForRead(subscription);
        var isActive = currentStatus == "Active";

        return Ok(new SubscriptionStatusDto
        {
            SubscriptionId = subscription.Id,
            Status = currentStatus,
            IsActive = isActive,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate
        });
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateSubscriptionStatus(int id, SubscriptionStatusUpdateDto dto)
    {
        var subscription = await _context.Subscriptions.FindAsync(id);
        if (subscription == null)
        {
            return NotFound();
        }

        var normalizedStatus = dto.Status.Trim();
        if (normalizedStatus.Equals("Active", StringComparison.OrdinalIgnoreCase) &&
            subscription.EndDate.Date < DateTime.Today)
        {
            return BadRequest(new { message = "Cannot set Active status for an expired subscription. Please extend it first." });
        }

        subscription.Status = ToCanonicalStatus(normalizedStatus);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> CancelSubscription(int id)
    {
        var subscription = await _context.Subscriptions.FindAsync(id);
        if (subscription == null)
        {
            return NotFound();
        }

        subscription.Status = "Cancelled";
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static SubscriptionResponseDto MapToResponse(Subscription s)
    {
        return new SubscriptionResponseDto
        {
            Id = s.Id,
            MemberId = s.MemberId,
            MemberCode = s.Member?.MemberCode ?? string.Empty,
            MemberName = s.Member?.FullName ?? string.Empty,
            MembershipPackageId = s.MembershipPackageId,
            PackageCode = s.MembershipPackage?.PackageCode ?? string.Empty,
            PackageName = s.MembershipPackage?.Name ?? string.Empty,
            PackageDurationDays = s.MembershipPackage?.DurationDays ?? 0,
            PackagePrice = s.MembershipPackage?.Price ?? 0,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            Status = NormalizeStatusForRead(s),
            CreatedAt = s.CreatedAt
        };
    }

    private static string NormalizeStatusForRead(Subscription subscription)
    {
        if (subscription.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
        {
            return "Cancelled";
        }

        return subscription.EndDate.Date >= DateTime.Today ? "Active" : "Expired";
    }

    private static string ToCanonicalStatus(string status)
    {
        if (status.Equals("Active", StringComparison.OrdinalIgnoreCase)) return "Active";
        if (status.Equals("Expired", StringComparison.OrdinalIgnoreCase)) return "Expired";
        if (status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)) return "Cancelled";
        return status;
    }
}
