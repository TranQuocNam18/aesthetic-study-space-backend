using AestheticStudySpace.Domain.Enums;

namespace AestheticStudySpace.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public AccountTier AccountTier { get; set; } = AccountTier.Free;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserRoomConfig> RoomConfigs { get; set; } = new List<UserRoomConfig>();
    public ICollection<Todo> Todos { get; set; } = new List<Todo>();
    public ICollection<PomodoroSession> PomodoroSessions { get; set; } = new List<PomodoroSession>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
