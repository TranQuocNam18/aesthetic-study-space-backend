namespace AestheticStudySpace.Application.DTOs.Admin;

public record AdminUserDto(
    Guid Id,
    string Username,
    string Email,
    string Role,
    string AccountTier,
    bool IsBanned,
    int CoinsBalance,
    DateTime CreatedAt,
    DateTime? LastLoginAt);

