using System.Security.Cryptography;
using System.Text;
using AestheticStudySpace.Application.DTOs.Auth;
using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Application.Interfaces;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Application.Mapping;
using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Exceptions;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace AestheticStudySpace.Application.Services;

public class AuthService : IAuthService
{
    private const string DefaultUserRoleName = "User";

    private readonly IRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IEmailSender _emailSender;
    private readonly IMissionService _missionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string? _frontendBaseUrl;
    private readonly string? _googleClientId;

    public AuthService(
        IRoleRepository roleRepository,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IEmailSender emailSender,
        IMissionService missionService,
        IConfiguration configuration,
        IOptions<AestheticStudySpace.Application.Common.GoogleAuthSettings> googleAuth,
        IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _emailSender = emailSender;
        _missionService = missionService;
        _unitOfWork = unitOfWork;

        _frontendBaseUrl = configuration["App:FrontendBaseUrl"];
        _googleClientId = googleAuth.Value.ClientId;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateRegistration(request);

        if (await _userRepository.GetByEmailAsync(request.Email, cancellationToken) is not null)
            throw new ValidationException("Email is already registered.");

        if (await _userRepository.GetByUsernameAsync(request.Username, cancellationToken) is not null)
            throw new ValidationException("Username is already taken.");

        var role = await _roleRepository.GetByNameAsync(DefaultUserRoleName, cancellationToken)
            ?? throw new InvalidOperationException($"Default role '{DefaultUserRoleName}' was not seeded.");

        var user = new User
        {
            Username = request.Username.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            RoleId = role.Id,
            Role = role
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _missionService.IncrementByTriggerKeyAsync(user.Id, "daily_login", 1, cancellationToken);
        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken)
            ?? throw new UnauthorizedException("Invalid email or password.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _missionService.IncrementByTriggerKeyAsync(user.Id, "daily_login", 1, cancellationToken);
        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var stored = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken)
            ?? throw new UnauthorizedException("Invalid refresh token.");

        if (stored.IsRevoked || stored.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedException("Refresh token expired or revoked.");

        stored.IsRevoked = true;
        await _refreshTokenRepository.UpdateAsync(stored, cancellationToken);

        var user = await _userRepository.GetByIdAsync(stored.UserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
            throw new ValidationException("IdToken is required.");

        if (string.IsNullOrWhiteSpace(_googleClientId))
            throw new InvalidOperationException("GoogleAuth:ClientId is not configured.");

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _googleClientId }
            });
        }
        catch (InvalidJwtException ex)
        {
            throw new UnauthorizedException($"Invalid Google ID token: {ex.Message}");
        }

        var email = payload.Email?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
            throw new ValidationException("Google token did not contain an email.");

        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            var role = await _roleRepository.GetByNameAsync(DefaultUserRoleName, cancellationToken)
                ?? throw new InvalidOperationException($"Default role '{DefaultUserRoleName}' was not seeded.");

            var baseUsername = (payload.Name ?? email.Split('@')[0]).Trim();
            baseUsername = string.IsNullOrWhiteSpace(baseUsername) ? "user" : baseUsername;

            var username = await EnsureUniqueUsernameAsync(baseUsername, cancellationToken);

            user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = _passwordHasher.Hash(Guid.NewGuid().ToString("N")), // random; user uses Google login
                RoleId = role.Id,
                Role = role,
                AvatarUrl = payload.Picture,
                LastLoginAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        await _missionService.IncrementByTriggerKeyAsync(user.Id, "daily_login", 1, cancellationToken);
        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ValidationException("Email is required.");

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        // Do not reveal whether the user exists.
        if (user is null)
            return;

        var token = GenerateToken();
        var tokenHash = Sha256Hex(token);

        var link = string.IsNullOrWhiteSpace(_frontendBaseUrl)
            ? token
            : $"{_frontendBaseUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(token)}";

        var body = $"""
<p>You requested a password reset for <strong>Aesthetic Study Space</strong>.</p>
<p>Reset link (valid for 30 minutes):</p>
<p><a href="{link}">{link}</a></p>
<p>If you didn't request this, you can ignore this email.</p>
""";

        // Send email BEFORE saving token to database
        // This way, if email fails, token won't be created unnecessarily
        await _emailSender.SendAsync(email, "Reset your password", body, cancellationToken);

        // Only save token if email was sent successfully
        var reset = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };

        await _passwordResetTokenRepository.AddAsync(reset, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            throw new ValidationException("Token is required.");

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
            throw new ValidationException("NewPassword must be at least 8 characters.");

        var tokenHash = Sha256Hex(request.Token.Trim());

        var stored = await _passwordResetTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken)
            ?? throw new UnauthorizedException("Invalid reset token.");

        if (stored.UsedAt is not null)
            throw new UnauthorizedException("Reset token already used.");

        if (stored.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedException("Reset token expired.");

        var user = await _userRepository.GetByIdAsync(stored.UserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        stored.UsedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _passwordResetTokenRepository.UpdateAsync(stored, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<AuthResponseDto> IssueTokensAsync(User user, CancellationToken cancellationToken)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenValue = _tokenService.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            TokenHash = CryptoUtils.Sha256Hex(refreshTokenValue),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            user.Id,
            user.Username,
            user.Email,
            user.Role.Name,
            user.AccountTier.ToString(),
            user.AvatarUrl,
            accessToken,
            refreshTokenValue,
            _tokenService.GetAccessTokenExpiration());
    }

    private static void ValidateRegistration(RegisterRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Length < 3)
            errors["username"] = new[] { "Username must be at least 3 characters." };

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
            errors["email"] = new[] { "A valid email is required." };

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            errors["password"] = new[] { "Password must be at least 8 characters." };

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }

    private async Task<string> EnsureUniqueUsernameAsync(string baseUsername, CancellationToken cancellationToken)
    {
        var cleaned = baseUsername.Trim();
        if (cleaned.Length > 30) cleaned = cleaned[..30];

        var candidate = cleaned;
        for (var i = 0; i < 20; i++)
        {
            if (await _userRepository.GetByUsernameAsync(candidate, cancellationToken) is null)
                return candidate;

            candidate = $"{cleaned}{RandomNumberGenerator.GetInt32(10, 9999)}";
            if (candidate.Length > 50) candidate = candidate[..50];
        }

        return $"{cleaned}{Guid.NewGuid():N}"[..50];
    }

    private static string GenerateToken()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string Sha256Hex(string value) => CryptoUtils.Sha256Hex(value);

    public async Task<UpdateUsernameResponseDto> UpdateUsernameAsync(Guid userId, UpdateUsernameRequestDto request, CancellationToken cancellationToken = default)
    {
        var newUsername = request.NewUsername?.Trim() ?? string.Empty;

        if (newUsername.Length < 3 || newUsername.Length > 30)
            throw new ValidationException("Username must be between 3 and 30 characters.");

        var existing = await _userRepository.GetByUsernameAsync(newUsername, cancellationToken);
        if (existing is not null && existing.Id != userId)
            throw new ValidationException("Username is already taken.");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        user.Username = newUsername;
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateUsernameResponseDto(user.Username);
    }
}
