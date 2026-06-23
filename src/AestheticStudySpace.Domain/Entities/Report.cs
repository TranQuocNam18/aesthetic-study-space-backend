namespace AestheticStudySpace.Domain.Entities;

public class Report : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    /// <summary>Type of report: "Feedback" or "Bug"</summary>
    public string Type { get; set; } = "Feedback";

    public string? AttachmentUrl { get; set; }

    /// <summary>Status of the report: "Pending", "InProgress", "Resolved", "Dismissed"</summary>
    public string Status { get; set; } = "Pending";
}
