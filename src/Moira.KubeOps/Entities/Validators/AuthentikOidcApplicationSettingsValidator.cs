using FluentValidation;
using Moira.Authentik.KubeOps.Entities;

namespace Moira.KubeOps.Entities.Validators;

internal class AuthentikOidcApplicationSettingsValidator : AbstractValidator<AuthentikOidcApplicationSettings>
{
    public AuthentikOidcApplicationSettingsValidator()
    {
        RuleFor(s => s.Spec.AuthorizationFlowSlug)
            .NotEmpty()
            .WithMessage("The \"authorizationFlowSlug\" property should not be empty");
        RuleFor(s => s.Spec.InvalidationFlowSlug)
            .NotEmpty()
            .WithMessage("The \"invalidationFlowSlug\" property should not be empty");
        RuleFor(s => s.Spec.RedirectUriMatchingMode)
            .NotEmpty()
            .Must(mode => mode.Equals("strict", StringComparison.OrdinalIgnoreCase) ||
                          mode.Equals("regex", StringComparison.OrdinalIgnoreCase))
            .WithMessage("The \"redirectUriMatchingMode\" property should be either \"strict\" or \"regex\"");
    }
}
