using ConnectDB.Data;
using ConnectDB.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConnectDB.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReportsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboard()
    {
        var today = DateTime.Today;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var nextMonth = monthStart.AddMonths(1);

        var activeMembers = await _context.Members.CountAsync(m => m.IsActive);
        var activePackages = await _context.MembershipPackages.CountAsync(p => p.IsActive);
        var activeSubscriptions = await _context.Subscriptions.CountAsync(s =>
            s.Status == "Active" && s.EndDate >= today);

        var monthlyRevenue = await _context.Payments
            .Where(p => p.Status == "Paid" && p.PaymentDate >= monthStart && p.PaymentDate < nextMonth)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        var totalSessionsHeld = await _context.Schedules.CountAsync(s => s.ScheduleDate <= today);
        var totalAttendanceRecords = await _context.Attendances.CountAsync();

        var expiring = await _context.Subscriptions
            .Include(s => s.Member)
            .Include(s => s.MembershipPackage)
            .Where(s => s.Status == "Active" &&
                        s.EndDate >= today &&
                        s.EndDate <= today.AddDays(7))
            .OrderBy(s => s.EndDate)
            .Select(s => new ExpiringSubscriptionDto
            {
                SubscriptionId = s.Id,
                MemberCode = s.Member!.MemberCode,
                MemberName = s.Member!.FullName,
                PackageName = s.MembershipPackage!.Name,
                EndDate = s.EndDate,
                DaysRemaining = (s.EndDate - today).Days
            })
            .ToListAsync();

        var result = new DashboardStatsDto
        {
            ActiveMembers = activeMembers,
            ActivePackages = activePackages,
            ActiveSubscriptions = activeSubscriptions,
            MonthlyRevenue = monthlyRevenue,
            TotalSessionsHeld = totalSessionsHeld,
            TotalAttendanceRecords = totalAttendanceRecords,
            ExpiringSubscriptions = expiring
        };

        return Ok(result);
    }
}
