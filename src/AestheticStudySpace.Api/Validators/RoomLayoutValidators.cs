using AestheticStudySpace.Application.DTOs.RoomLayouts;
using FluentValidation;

namespace AestheticStudySpace.Api.Validators;

public class SaveRoomLayoutRequestDtoValidator : AbstractValidator<SaveRoomLayoutRequestDto>
{
    public SaveRoomLayoutRequestDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Layout name is required.")
            .MaximumLength(120).WithMessage("Layout name cannot exceed 120 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.LayoutJson)
            .NotEmpty().WithMessage("Layout JSON configuration is required.");
    }
}
