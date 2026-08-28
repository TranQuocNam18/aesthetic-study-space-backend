namespace AestheticStudySpace.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public AestheticStudySpace.Domain.Enums.AccountTier AccountTier { get; set; } = AestheticStudySpace.Domain.Enums.AccountTier.Free;

    public int CoinsBalance { get; set; }
    public bool IsBanned { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LastRetentionEmailSentAt { get; set; }

    public string? DefaultBankAccountNumber { get; set; }
    public string? DefaultBankName { get; set; }
    public string? DefaultBankAccountOwnerName { get; set; }

    public ICollection<UserRoomConfig> RoomConfigs { get; set; } = new List<UserRoomConfig>();
    public ICollection<Todo> Todos { get; set; } = new List<Todo>();
    public ICollection<PomodoroSession> PomodoroSessions { get; set; } = new List<PomodoroSession>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
