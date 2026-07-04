using FluentValidation;
using Moira.Authentik.Kubernetes.Group;

namespace Moira.Authentik.Kubernetes.ValidatorWebhooks.Validators;

internal class AuthentikGroupValidator : AbstractValidator<AuthentikGroup>
{
    public AuthentikGroupValidator()
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
        RuleFor(g => g.Spec.ProviderRef.Name).NotEmpty();
        RuleFor(g => g.Spec.ProviderRef.Namespace).NotEmpty();
    }
}
