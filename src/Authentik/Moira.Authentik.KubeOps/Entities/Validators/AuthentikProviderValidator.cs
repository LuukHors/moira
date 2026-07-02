using FluentValidation;

namespace Moira.Authentik.KubeOps.Entities.Validators;

public class AuthentikProviderValidator : AbstractValidator<AuthentikProvider>
{
    public AuthentikProviderValidator()
    {
        RuleFor(p => p.Spec.BaseUrl)
            .NotNull()
            .NotEmpty();
        RuleFor(p => p.Spec.SecretRef)
            .NotNull();
        RuleFor(p => p.Spec.SecretRef.Name)
            .NotNull()
            .NotEmpty();
        RuleFor(p => p.Spec.SecretRef.NamespaceProperty)
            .NotNull()
            .NotEmpty();
    }
}
