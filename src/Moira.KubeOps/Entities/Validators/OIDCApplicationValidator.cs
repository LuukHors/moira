using FluentValidation;

namespace Moira.KubeOps.Entities.Validators;

internal class OIDCApplicationValidator : AbstractValidator<OidcApplication>
{
    public OIDCApplicationValidator()
    {
        RuleFor(a => a.Spec.DisplayName)
            .NotEmpty()
            .WithMessage("The OIDC application's displayName should not be empty");
        RuleFor(a => a.Spec.ProviderRef)
            .NotNull()
            .WithMessage("The \"providerRef\" property should be set");
        RuleFor(a => a.Spec.ProviderRef.Name)
            .NotEmpty()
            .WithMessage("The \"providerRef.name\" property should be set");
        RuleFor(a => a.Spec.ProviderRef.Namespace)
            .NotEmpty()
            .WithMessage("The \"providerRef.namespace\" property should be set");
        RuleFor(a => a.Spec.RotationDays)
            .GreaterThan(0)
            .WithMessage("The \"rotationDays\" property should be greater than 0");
        RuleFor(a => a.Spec.Secrets)
            .Must(HaveUniqueSecrets)
            .WithMessage("Secrets should be unique by cluster, namespace and name");

        RuleForEach(a => a.Spec.Secrets).ChildRules(secret =>
        {
            secret.RuleFor(t => t.Name)
                .NotEmpty()
                .Matches(@"^[a-z0-9]([-a-z0-9]*[a-z0-9])?$")
                .WithMessage("Secret name should be a valid Kubernetes name");
            secret.RuleFor(t => t.Namespace)
                .NotEmpty()
                .Matches(@"^[a-z0-9]([-a-z0-9]*[a-z0-9])?$")
                .WithMessage("Secret namespace should be a valid Kubernetes name");
            secret.When(t => t.Template is not null, () =>
            {
                secret.RuleForEach(t => t.Template).ChildRules(template =>
                {
                    template.RuleFor(t => t.Key)
                        .NotEmpty()
                        .Matches(@"^[-._a-zA-Z0-9]+$")
                        .WithMessage("Secret template key should be a valid Kubernetes secret data key");
                    template.RuleFor(t => t.Value)
                        .NotEmpty()
                        .WithMessage("Secret template value should not be empty");
                });
            });
            secret.When(t => t.ClusterSecretRef is not null, () =>
            {
                secret.RuleFor(t => t.ClusterSecretRef!.Name)
                    .NotEmpty()
                    .WithMessage("Remote secrets should set clusterRef.kubeConfigSecretRef.name");
                secret.RuleFor(t => t.ClusterSecretRef!.Namespace)
                    .NotEmpty()
                    .WithMessage("Remote secrets should set clusterRef.kubeConfigSecretRef.namespace");
                secret.RuleFor(t => t.ClusterSecretRef!.Key)
                    .NotEmpty()
                    .WithMessage("Remote secrets should set clusterRef.kubeConfigSecretRef.key");
            });
        });
    }

    private static bool HaveUniqueSecrets(IEnumerable<OidcApplication.Secret>? secrets)
    {
        if (secrets is null)
        {
            return true;
        }

        var keys = secrets.Select(secret =>
        {
            var cluster = secret.ClusterSecretRef is null
                ? "local"
                : $"{secret.ClusterSecretRef.Namespace}/{secret.ClusterSecretRef.Name}/{secret.ClusterSecretRef.Key}";
            return $"{cluster}/{secret.Namespace}/{secret.Name}";
        });

        return keys.Distinct(StringComparer.OrdinalIgnoreCase).Count() == secrets.Count();
    }
}
