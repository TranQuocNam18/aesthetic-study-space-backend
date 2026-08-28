using AestheticStudySpace.Application.DTOs.Pomodoro;
using FluentValidation;

namespace AestheticStudySpace.Api.Validators;

public class StartPomodoroRequestDtoValidator : AbstractValidator<StartPomodoroRequestDto>
{
    public StartPomodoroRequestDtoValidator()
    {
        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(1, 120).WithMessage("Duration must be between 1 and 120 minutes.");
    }
}

public class EndPomodoroRequestDtoValidator : AbstractValidator<EndPomodoroRequestDto>
{
    public EndPomodoroRequestDtoValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required.");
    }
}

public class CancelPomodoroRequestDtoValidator : AbstractValidator<CancelPomodoroRequestDto>
{
    public CancelPomodoroRequestDtoValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required.");
    }
}
