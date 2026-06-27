using Moira.Authentik.Application.Ports;
using Moira.Authentik.Domain.Applications;
using Moira.Common.Exceptions;
using Moira.Common.Models;

namespace Moira.Authentik.Application.Builders;

public class AuthentikOAuth2ProviderBuilder(
    IAuthentikRepository<AuthentikScopeMappingV3, AuthentikScopeMappingV3, string> scopeMappingRepository,
    IAuthentikRepository<AuthentikFlowV3, AuthentikFlowV3, string> flowRepository) : IAuthentikOAuth2ProviderBuilder
{
    private const string DefaultAuthorizationFlowSlug = "default-provider-authorization-explicit-consent";
    private const string DefaultInvalidationFlowSlug = "default-provider-invalidation-flow";
    private const string DefaultRedirectUriMatchingMode = "strict";
    private static readonly IReadOnlyDictionary<string, object> DefaultAttributes = new Dictionary<string, object> { ["managed-by"] = "moira" };

    public async Task<AuthentikOAuth2ProviderV3> BuildAsync(
        IdPOidcApplication application,
        string clientId,
        string clientSecret,
        int? providerId,
        CancellationToken cancellationToken)
    {
        ValidateSupportedCoreProperties(application);

        var authorizationFlowSlug = application.Spec.ProviderSettings?.GetValueOrDefault(
            "authorizationFlowSlug",
            DefaultAuthorizationFlowSlug) ?? DefaultAuthorizationFlowSlug;
        var invalidationFlowSlug = application.Spec.ProviderSettings?.GetValueOrDefault(
            "invalidationFlowSlug",
            DefaultInvalidationFlowSlug) ?? DefaultInvalidationFlowSlug;
        var redirectUriMatchingMode = application.Spec.ProviderSettings?.GetValueOrDefault(
            "redirectUriMatchingMode",
            DefaultRedirectUriMatchingMode) ?? DefaultRedirectUriMatchingMode;

        var authorizationFlowTask = GetFlowIdAsync(application.IdPProvider, authorizationFlowSlug, cancellationToken);
        var invalidationFlowTask = GetFlowIdAsync(application.IdPProvider, invalidationFlowSlug, cancellationToken);
        var scopeMappingIdsTask = ResolveScopeMappingIdsAsync(application, cancellationToken);

        await Task.WhenAll(authorizationFlowTask, invalidationFlowTask, scopeMappingIdsTask);

        return new AuthentikOAuth2ProviderV3
        {
            name = application.Spec.DisplayName,
            pk = providerId,
            client_type = application.Spec.UsesClientSecret ? "confidential" : "public",
            client_id = clientId,
            client_secret = application.Spec.UsesClientSecret ? clientSecret : string.Empty,
            authorization_flow = authorizationFlowTask.Result,
            invalidation_flow = invalidationFlowTask.Result,
            attributes = DefaultAttributes,
            property_mappings = scopeMappingIdsTask.Result.Cast<object>().ToArray(),
            logout_uri = application.Spec.LogoutUri,
            redirect_uris = application.Spec.RedirectUris
                .Select(uri => new AuthentikRedirectUriV3(redirectUriMatchingMode, uri))
        };
    }

    private async Task<string> GetFlowIdAsync(IdPProvider provider, string flowSlug, CancellationToken cancellationToken)
    {
        var flow = await flowRepository.GetByIdAsync(flowSlug, provider, null, cancellationToken);

        if (flow is null || string.IsNullOrWhiteSpace(flow.pk))
        {
            throw new IdPException(
                $"Authentik flow \"{flowSlug}\" was not found.",
                IdPExceptionReason.IdpValidationFailed);
        }

        return flow.pk;
    }

    private async Task<IEnumerable<string>> ResolveScopeMappingIdsAsync(IdPOidcApplication application, CancellationToken cancellationToken)
    {
        var scopeNames = application.Spec.Scopes
            .Where(scope => !string.IsNullOrWhiteSpace(scope))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var scopeMappingTasks = scopeNames.Select(scope => ResolveScopeMappingIdAsync(application, scope, cancellationToken));

        var scopeMappingIds = await Task.WhenAll(scopeMappingTasks);
        return scopeMappingIds.Order(StringComparer.OrdinalIgnoreCase);
    }

    private async Task<string> ResolveScopeMappingIdAsync(IdPOidcApplication application, string scope, CancellationToken cancellationToken)
    {
        var page = await scopeMappingRepository.ListByQueryAsync(
            new Dictionary<string, string> { ["scope_name"] = scope },
            application.IdPProvider,
            cancellationToken: cancellationToken);
        var matches = page.Results
            .Where(mapping => mapping.scope_name.Equals(scope, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (matches.Length == 0)
        {
            throw new IdPException(
                $"Authentik scope mapping for scope \"{scope}\" was not found.",
                IdPExceptionReason.IdpValidationFailed);
        }

        if (matches.Length > 1)
        {
            throw new IdPException(
                $"Found multiple Authentik scope mappings for scope \"{scope}\". Use unique scope names before reconciling this OIDC application.",
                IdPExceptionReason.IdpValidationFailed);
        }

        return matches[0].pk;
    }

    private static void ValidateSupportedCoreProperties(IdPOidcApplication application)
    {
        if (!string.IsNullOrWhiteSpace(application.Spec.ClientUri) ||
            !string.IsNullOrWhiteSpace(application.Spec.LogoUri) ||
            !string.IsNullOrWhiteSpace(application.Spec.PolicyUri) ||
            !string.IsNullOrWhiteSpace(application.Spec.TermsOfServiceUri) ||
            application.Spec.Contacts.Any())
        {
            throw new IdPException(
                "Authentik OIDC application support currently does not reconcile OIDC client metadata fields such as clientUri, logoUri, policyUri, termsOfServiceUri or contacts.",
                IdPExceptionReason.IdpValidationFailed);
        }
    }
}