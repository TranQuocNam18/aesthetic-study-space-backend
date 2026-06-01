namespace AestheticStudySpace.Application.DTOs.Auth;

public record RegisterRequestDto(string Username, string Email, string Password);

public record LoginRequestDto(string Email, string Password);

public record RefreshTokenRequestDto(string RefreshToken);

public record GoogleLoginRequestDto(string IdToken);

public record ForgotPasswordRequestDto(string Email);

public record ResetPasswordRequestDto(string Token, string NewPassword);

public record UpdateUsernameRequestDto(string NewUsername);

public record UpdateUsernameResponseDto(string NewUsername);

public record AuthResponseDto(
    Guid UserId,
    string Username,
    string Email,
    string Role,
    string AccountTier,
    string? AvatarUrl,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt);

public record UserProfileDto(
    Guid Id,
    string Username,
    string Email,
    string Role,
    string AccountTier,
    string? AvatarUrl,
    DateTime CreatedAt);
