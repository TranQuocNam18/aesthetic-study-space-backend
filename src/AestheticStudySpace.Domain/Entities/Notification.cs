namespace AestheticStudySpace.Domain.Entities;

public class Notification : BaseEntity
{
    /// <summary>
    /// The user to receive this notification. Can be null for notifications targeted broadly (e.g. to admins).
    /// </summary>
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    /// <summary>
    /// Flags if this notification is targeted at administrators.
    /// </summary>
    public bool IsForAdmin { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
}
