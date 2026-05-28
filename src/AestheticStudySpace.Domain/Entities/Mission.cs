namespace AestheticStudySpace.Domain.Entities;

public class Mission : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int RewardCoins { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// e.g. daily_login, pomodoro_complete, study_minutes, weekly_goal
    /// </summary>
    public string TriggerKey { get; set; } = string.Empty;

    /// <summary>
    /// Used for missions that require progress accumulation.
    /// </summary>
    public int? TargetValue { get; set; }

    /// <summary>
    /// daily/weekly/once
    /// </summary>
    public string Frequency { get; set; } = "daily";
}

