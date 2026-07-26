namespace AestheticStudySpace.Application.Common;

public static class MissionPeriodHelper
{
    public static DateOnly GetPeriodDate(string? frequency, DateTime? utcNow = null)
    {
        var freq = string.IsNullOrWhiteSpace(frequency) ? "daily" : frequency.Trim().ToLowerInvariant();
        var now = utcNow ?? DateTime.UtcNow;
        return freq switch
        {
            "weekly" => GetWeekStart(DateOnly.FromDateTime(now)),
            "once" => DateOnly.FromDateTime(DateTime.UnixEpoch),
            "rolling_weekly" => DateOnly.FromDateTime(now),
            "daily_login_streak" => DateOnly.FromDateTime(now),
            _ => DateOnly.FromDateTime(now)
        };
    }

    public static bool IsPeriodValid(string? frequency, DateOnly periodDate, DateOnly currentDate)
    {
        var freq = string.IsNullOrWhiteSpace(frequency) ? "daily" : frequency.Trim().ToLowerInvariant();
        return freq switch
        {
            "daily" => periodDate == currentDate,
            "weekly" => periodDate == GetWeekStart(currentDate),
            "rolling_weekly" => currentDate >= periodDate && currentDate < periodDate.AddDays(7),
            "daily_login_streak" => currentDate >= periodDate && currentDate <= periodDate.AddDays(1),
            "once" => true,
            _ => periodDate == currentDate
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

