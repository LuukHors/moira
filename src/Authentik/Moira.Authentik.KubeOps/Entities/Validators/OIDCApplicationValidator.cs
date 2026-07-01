using FluentValidation;
using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.KubeOps.Entities.Validators;

internal class OIDCApplicationValidator : AbstractValidator<AuthentikOidcApplication>
{
    private static readonly ISet<string> GrantTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "authorization_code",
        "implicit",
        "refresh_token",
        "client_credentials"
    };

    private static readonly ISet<string> ResponseTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "code",
        "id_token",
        "id_token token",
        "code id_token",
        "code token",
        "code id_token token"
    };

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
        RuleFor(a => a.Spec.AuthorizationFlowSlug)
            .NotEmpty()
            .WithMessage("The \"authorizationFlowSlug\" property should not be empty");
        RuleFor(a => a.Spec.InvalidationFlowSlug)
            .NotEmpty()
            .WithMessage("The \"invalidationFlowSlug\" property should not be empty");
        RuleFor(a => a.Spec.RedirectUriMatchingMode)
            .NotEmpty()
            .Must(mode => mode.Equals("strict", StringComparison.OrdinalIgnoreCase) ||
                          mode.Equals("regex", StringComparison.OrdinalIgnoreCase))
            .WithMessage("The \"redirectUriMatchingMode\" property should be either \"strict\" or \"regex\"");
        RuleFor(a => a.Spec.Oidc)
            .NotNull()
            .WithMessage("The \"oidc\" property should be set");
        RuleFor(a => a.Spec.Oidc.ApplicationType)
            .Must(t => t != OidcApplicationType.Unknown && Enum.IsDefined(t))
            .WithMessage("The \"oidc.applicationType\" property should be either \"Web\" or \"Native\"");
        RuleFor(a => a.Spec.Oidc.ClientAuthenticationMethod)
            .Must(t => t != OidcClientAuthenticationMethod.Unknown && Enum.IsDefined(t))
            .WithMessage("The \"oidc.clientAuthenticationMethod\" property should be \"ClientSecretBasic\", \"ClientSecretPost\" or \"None\"");
        RuleFor(a => a.Spec.Oidc.Scopes)
            .Must(scopes => HasValue(scopes, "openid"))
            .WithMessage("The \"oidc.scopes\" property should include \"openid\"");
        RuleForEach(a => a.Spec.Oidc.GrantTypes)
            .Must(t => GrantTypes.Contains(t))
            .WithMessage("The \"oidc.grantTypes\" property contains an unsupported grant type");
        RuleForEach(a => a.Spec.Oidc.ResponseTypes)
            .Must(t => ResponseTypes.Contains(NormalizeResponseType(t)))
            .WithMessage("The \"oidc.responseTypes\" property contains an unsupported response type");
        RuleFor(a => a.Spec.Oidc)
            .Must(HaveRedirectUrisWhenNeeded)
            .WithMessage("The \"oidc.redirectUris\" property should be set when using redirect-based OIDC flows");
        RuleFor(a => a.Spec.Oidc)
            .Must(HaveMatchingGrantTypesForResponseTypes)
            .WithMessage("The \"oidc.grantTypes\" property should include the grants required by \"oidc.responseTypes\"");
        RuleForEach(a => a.Spec.Oidc.RedirectUris)
            .Must(IsAbsoluteUri)
            .WithMessage("The \"oidc.redirectUris\" property should contain absolute URIs");
        RuleFor(a => a.Spec.Oidc.LogoutUri)
            .Must(BeEmptyOrAbsoluteUri)
            .WithMessage("The \"oidc.logoutUri\" property should be an absolute URI");
        RuleFor(a => a.Spec.Oidc.LaunchUrl)
            .Must(BeEmptyOrAbsoluteUri)
            .WithMessage("The \"oidc.launchUrl\" property should be an absolute URI");
        RuleFor(a => a.Spec.Oidc.ClientUri)
            .Must(BeEmptyOrAbsoluteUri)
            .WithMessage("The \"oidc.clientUri\" property should be an absolute URI");
        RuleFor(a => a.Spec.Oidc.LogoUri)
            .Must(BeEmptyOrAbsoluteUri)
            .WithMessage("The \"oidc.logoUri\" property should be an absolute URI");
        RuleFor(a => a.Spec.Oidc.PolicyUri)
            .Must(BeEmptyOrAbsoluteUri)
            .WithMessage("The \"oidc.policyUri\" property should be an absolute URI");
        RuleFor(a => a.Spec.Oidc.TermsOfServiceUri)
            .Must(BeEmptyOrAbsoluteUri)
            .WithMessage("The \"oidc.termsOfServiceUri\" property should be an absolute URI");
        RuleForEach(a => a.Spec.Oidc.Contacts)
            .EmailAddress()
            .WithMessage("The \"oidc.contacts\" property should contain valid email addresses");
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

    private static bool HaveUniqueSecrets(IEnumerable<AuthentikOidcApplication.Secret>? secrets)
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

    private static bool HasValue(IEnumerable<string>? values, string expected)
    {
        return values?.Any(value => value.Equals(expected, StringComparison.OrdinalIgnoreCase)) == true;
    }

    private static bool HaveRedirectUrisWhenNeeded(OidcSpec spec)
    {
        var grants = spec.GrantTypes?.ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];
        var responseTypes = spec.ResponseTypes?.Select(NormalizeResponseType).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

        var needsRedirectUris =
            grants.Contains("authorization_code") ||
            grants.Contains("implicit") ||
            responseTypes.Any(responseType => responseType.Contains("code", StringComparison.OrdinalIgnoreCase) ||
                                              responseType.Contains("id_token", StringComparison.OrdinalIgnoreCase) ||
                                              responseType.Contains("token", StringComparison.OrdinalIgnoreCase));

        return !needsRedirectUris || spec.RedirectUris?.Any() == true;
    }

    private static bool HaveMatchingGrantTypesForResponseTypes(OidcSpec spec)
    {
        var grants = spec.GrantTypes?.ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];
        var responseTypes = spec.ResponseTypes?.Select(NormalizeResponseType) ?? [];

        foreach (var responseType in responseTypes)
        {
            var parts = responseType.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Contains("code") && !grants.Contains("authorization_code"))
            {
                return false;
            }

            if ((parts.Contains("id_token") || parts.Contains("token")) && !grants.Contains("implicit"))
            {
                return false;
            }
        }

        return true;
    }

    private static string NormalizeResponseType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return string.Join(
            ' ',
            value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private static bool BeEmptyOrAbsoluteUri(string? value)
    {
        return string.IsNullOrWhiteSpace(value) || IsAbsoluteUri(value);
    }

    private static bool IsAbsoluteUri(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out _);
    }
}
