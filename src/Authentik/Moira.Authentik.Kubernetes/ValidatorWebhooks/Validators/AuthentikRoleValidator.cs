using FluentValidation;
using Moira.Authentik.Kubernetes.Role;

namespace Moira.Authentik.Kubernetes.ValidatorWebhooks.Validators;

internal class AuthentikRoleValidator : AbstractValidator<AuthentikRole>
{
    public AuthentikRoleValidator()
    {
        RuleFor(g => g.Spec.DisplayName)
            .NotNull()
            .WithMessage("The \"DisplayName\" property should be set");
    }
}
