using AestheticStudySpace.Application.DTOs.Auth;
using FluentValidation;

namespace AestheticStudySpace.Api.Validators;

public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestDtoValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3).MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(100);
    }
}

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(1).MaximumLength(100);
    }
}

public class RefreshTokenRequestDtoValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestDtoValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().MinimumLength(10);
    }
}

public class GoogleLoginRequestDtoValidator : AbstractValidator<GoogleLoginRequestDto>
{
    public GoogleLoginRequestDtoValidator()
    {
        RuleFor(x => x.IdToken).NotEmpty();
    }
}

public class ForgotPasswordRequestDtoValidator : AbstractValidator<ForgotPasswordRequestDto>
{
    public ForgotPasswordRequestDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
    }
}

public class ResetPasswordRequestDtoValidator : AbstractValidator<ResetPasswordRequestDto>
{
    public ResetPasswordRequestDtoValidator()
    {
        RuleFor(x => x.Token).NotEmpty().MinimumLength(10);
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(100);
    }
}

public class UpdateUsernameRequestDtoValidator : AbstractValidator<UpdateUsernameRequestDto>
{
    public UpdateUsernameRequestDtoValidator()
    {
        RuleFor(x => x.NewUsername)
            .NotEmpty().WithMessage("New username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters.")
            .MaximumLength(30).WithMessage("Username must be at most 30 characters.")
            .Matches(@"^[a-zA-Z0-9_\.]+$").WithMessage("Username can only contain letters, numbers, underscores and dots.");
    }
}

