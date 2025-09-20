using FluentValidation;

namespace Moira.KubeOps.Entities.Validators;

public class GroupValidator : AbstractValidator<Group>
{
    public GroupValidator()
    {
        RuleFor(g => g.Spec.ProviderRef)
            .NotNull()
            .WithMessage("The \"providerRef\" property should be set");
        RuleFor(g => g.Spec.ProviderRef.Name)
            .NotNull()
            .NotEmpty()
            .WithMessage("The \"providerRef.name\" property should be set");
        RuleFor(g => g.Spec.DisplayName)
            .NotEmpty()
            .WithMessage("The group's displayName should not be empty");
    }
}
