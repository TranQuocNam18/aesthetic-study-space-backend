using AestheticStudySpace.Application.DTOs.Workspace;
using FluentValidation;

namespace AestheticStudySpace.Api.Validators;

public class SaveWorkspaceRequestDtoValidator : AbstractValidator<SaveWorkspaceRequestDto>
{
    public SaveWorkspaceRequestDtoValidator()
    {
        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("Room ID is required.");

        RuleFor(x => x.JsonConfig)
            .NotEmpty().WithMessage("Workspace JSON configuration is required.");
    }
}
