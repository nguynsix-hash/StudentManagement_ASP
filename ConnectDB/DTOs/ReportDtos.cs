namespace ConnectDB.DTOs;

public class DashboardStatsDto
{
    public int ActiveMembers { get; set; }
    public int ActivePackages { get; set; }
    public int ActiveSubscriptions { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int TotalSessionsHeld { get; set; }
    public int TotalAttendanceRecords { get; set; }
    public List<ExpiringSubscriptionDto> ExpiringSubscriptions { get; set; } = new();
}

public class ExpiringSubscriptionDto
{
    public int SubscriptionId { get; set; }
    public string MemberCode { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public DateTime EndDate { get; set; }
    public int DaysRemaining { get; set; }
}
