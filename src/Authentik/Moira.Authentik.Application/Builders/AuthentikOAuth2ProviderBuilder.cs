using Moira.Authentik.Application.Models;
using Moira.Authentik.Domain.Applications;
using Moira.Authentik.Domain.ProviderSettings;
using Moira.Common;
using Moira.Common.Abstractions;
using Moira.Common.Abstractions.Exceptions;
using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Application.Builders;

public class AuthentikOAuth2ProviderBuilder(
    IAuthentikFlowBuilder flowBuilder,
    IAuthentikScopeMappingBuilder scopeMappingBuilder) : IAuthentikOAuth2ProviderBuilder
{
    private static readonly IReadOnlyDictionary<string, object> DefaultAttributes = new Dictionary<string, object> { ["managed-by"] = "moira" };

    public async Task<AuthentikOAuth2ProviderV3> BuildAsync(
        AuthentikOidcApplicationModel application,
        string clientId,
        string clientSecret,
        int? providerId,
        CancellationToken cancellationToken)
    {
        ValidateSupportedCoreProperties(application);

        var settings = application.Spec.Authentik;

        var authorizationFlowSlug = NonEmpty(settings.AuthorizationFlowSlug);
        var invalidationFlowSlug = NonEmpty(settings.InvalidationFlowSlug);
        var redirectUriMatchingMode = NonEmpty(settings.RedirectUriMatchingMode) ;

        var accessCodeValidity = NonEmpty(settings.TokenSettings.AccessCodeValidity);
        var accessTokenValidity = NonEmpty(settings.TokenSettings.AccessTokenValidity);
        var refreshTokenValidity = NonEmpty(settings.TokenSettings.RefreshTokenValidity);

        var authorizationFlowTask = flowBuilder.BuildAsync(application.IdPProvider, authorizationFlowSlug, cancellationToken);
        var invalidationFlowTask = flowBuilder.BuildAsync(application.IdPProvider, invalidationFlowSlug, cancellationToken);
        var scopeMappingIdsTask = scopeMappingBuilder.BuildAsync(application.Spec.Scopes, application.IdPProvider, cancellationToken);

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
                .Select(uri => new AuthentikRedirectUriV3(redirectUriMatchingMode, uri)),
            access_code_validity = accessCodeValidity,
            access_token_validity = accessTokenValidity, 
            refresh_token_validity = refreshTokenValidity,
        };

        static string? NonEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static void ValidateSupportedCoreProperties(AuthentikOidcApplicationModel application)
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