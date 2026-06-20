using FluentValidation;

namespace Moira.KubeOps.Entities.Validators;

internal class OIDCApplicationValidator : AbstractValidator<OidcApplication>
{
    internal OIDCApplicationValidator()
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
        RuleFor(a => a.Spec.SecretTargets)
            .NotEmpty()
            .WithMessage("At least one secret target should be configured")
            .Must(HaveUniqueTargets)
            .WithMessage("Secret targets should be unique by cluster, namespace and name");

        RuleForEach(a => a.Spec.SecretTargets).ChildRules(target =>
        {
            target.RuleFor(t => t.Name)
                .NotEmpty()
                .Matches(@"^[a-z0-9]([-a-z0-9]*[a-z0-9])?$")
                .WithMessage("Secret target name should be a valid Kubernetes name");
            target.RuleFor(t => t.Namespace)
                .NotEmpty()
                .Matches(@"^[a-z0-9]([-a-z0-9]*[a-z0-9])?$")
                .WithMessage("Secret target namespace should be a valid Kubernetes name");
            target.RuleFor(t => t.Keys.ClientId)
                .NotEmpty()
                .WithMessage("Secret target clientId key should be set");
            target.RuleFor(t => t.Keys.ClientSecret)
                .NotEmpty()
                .WithMessage("Secret target clientSecret key should be set");
            target.When(t => t.ClusterRef is not null, () =>
            {
                target.RuleFor(t => t.ClusterRef!.KubeConfigSecretRef.Name)
                    .NotEmpty()
                    .WithMessage("Remote secret targets should set clusterRef.kubeConfigSecretRef.name");
                target.RuleFor(t => t.ClusterRef!.KubeConfigSecretRef.Namespace)
                    .NotEmpty()
                    .WithMessage("Remote secret targets should set clusterRef.kubeConfigSecretRef.namespace");
                target.RuleFor(t => t.ClusterRef!.KubeConfigSecretRef.Key)
                    .NotEmpty()
                    .WithMessage("Remote secret targets should set clusterRef.kubeConfigSecretRef.key");
            });
        });
    }

    private static bool HaveUniqueTargets(IEnumerable<OidcApplication.SecretTarget> targets)
    {
        var keys = targets.Select(target =>
        {
            var cluster = target.ClusterRef is null
                ? "local"
                : $"{target.ClusterRef.KubeConfigSecretRef.Namespace}/{target.ClusterRef.KubeConfigSecretRef.Name}/{target.ClusterRef.KubeConfigSecretRef.Key}";
            return $"{cluster}/{target.Namespace}/{target.Name}";
        });

        return keys.Distinct(StringComparer.OrdinalIgnoreCase).Count() == targets.Count();
    }
}
