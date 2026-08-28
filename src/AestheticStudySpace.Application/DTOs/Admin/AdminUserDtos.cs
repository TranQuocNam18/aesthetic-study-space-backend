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

/// <summary>
/// Request body để admin cập nhật AccountTier của user.
/// Tier hợp lệ: "Free" hoặc "Premium"
/// </summary>
public record UpdateUserTierRequestDto(string Tier);

public record AddCoinsRequestDto(int Amount);

