using FluentValidation;

namespace Moira.KubeOps.Entities.Validators;

public class ProviderValidator : AbstractValidator<Provider>
{
    public ProviderValidator()
    {
        RuleFor(p => p.Spec.Type)
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
