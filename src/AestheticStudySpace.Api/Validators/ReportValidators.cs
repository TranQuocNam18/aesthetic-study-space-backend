using AestheticStudySpace.Application.DTOs.Report;
using FluentValidation;

namespace AestheticStudySpace.Api.Validators;

public class CreateReportRequestDtoValidator : AbstractValidator<CreateReportRequestDto>
{
    public CreateReportRequestDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Report title is required.")
            .MaximumLength(256).WithMessage("Report title cannot exceed 256 characters.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Report content is required.")
            .MaximumLength(4000).WithMessage("Report content cannot exceed 4000 characters.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Report type is required.")
            .Must(t => t == "Bug" || t == "Feedback")
            .WithMessage("Report type must be either 'Bug' or 'Feedback'.");
    }
}
