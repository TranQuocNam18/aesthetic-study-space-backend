using AestheticStudySpace.Application.DTOs.Todos;
using FluentValidation;

namespace AestheticStudySpace.Api.Validators;

public class CreateTodoRequestDtoValidator : AbstractValidator<CreateTodoRequestDto>
{
    public CreateTodoRequestDtoValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Todo content is required.")
            .MaximumLength(500).WithMessage("Todo content cannot exceed 500 characters.");
    }
}

public class UpdateTodoRequestDtoValidator : AbstractValidator<UpdateTodoRequestDto>
{
    public UpdateTodoRequestDtoValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Todo content is required.")
            .MaximumLength(500).WithMessage("Todo content cannot exceed 500 characters.");
    }
}
