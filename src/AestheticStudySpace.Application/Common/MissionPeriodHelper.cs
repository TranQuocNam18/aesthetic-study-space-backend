namespace AestheticStudySpace.Application.Common;

public static class MissionPeriodHelper
{
    public static DateOnly GetPeriodDate(string frequency, DateTime? utcNow = null)
    {
        var now = utcNow ?? DateTime.UtcNow;
        return frequency.Trim().ToLowerInvariant() switch
        {
            "weekly" => GetWeekStart(DateOnly.FromDateTime(now)),
            "once" => DateOnly.FromDateTime(DateTime.UnixEpoch),
            _ => DateOnly.FromDateTime(now)
        };
    }

    private static DateOnly GetWeekStart(DateOnly date)
    {
        var diff = (int)date.DayOfWeek - (int)DayOfWeek.Monday;
        if (diff < 0)
            diff += 7;
        return date.AddDays(-diff);
    }
}
