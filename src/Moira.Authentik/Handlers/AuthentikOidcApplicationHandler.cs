using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Moira.Authentik.HttpService;
using Moira.Authentik.Models.V3;
using Moira.Common.Commands;
using Moira.Common.Exceptions;
using Moira.Common.Models;

namespace Moira.Authentik.Handlers;

public partial class AuthentikOidcApplicationHandler(
    IHttpService<AuthentikApplicationV3, AuthentikApplicationV3, string> applicationHttpClient,
    IHttpService<AuthentikOAuth2ProviderV3, AuthentikOAuth2ProviderV3, int> providerHttpClient,
    IHttpService<AuthentikFlowV3, AuthentikFlowV3, string> flowHttpClient,
    ILogger<AuthentikOidcApplicationHandler> logger) : IAuthentikOidcApplicationHandler
{
    private const string DefaultAuthorizationFlowSlug = "default-provider-authorization-explicit-consent";
    private const string DefaultInvalidationFlowSlug = "default-provider-invalidation-flow";
    private const string DefaultRedirectUriMatchingMode = "strict";
    private static readonly IReadOnlyDictionary<string, object> DefaultAttributes = new Dictionary<string, object> { ["managed-by"] = "moira" };

    public async Task<AuthentikOidcApplicationV3?> GetAsync(IdPCommand<IdPOidcApplication> command, CancellationToken cancellationToken)
    {
        var application = await GetApplicationAsync(command, cancellationToken);
        if (application is null)
        {
            return null;
        }

        if (application.provider is null)
        {
            logger.LogInformation("Authentik application {ApplicationId} does not have an OAuth2 provider", application.slug);
            return new AuthentikOidcApplicationV3(application, null);
        }

        var provider = await providerHttpClient.GetByIdAsync(
            application.provider.Value,
            command.Entity.IdPProvider,
            DefaultAttributes,
            cancellationToken);

        return new AuthentikOidcApplicationV3(application, provider);
    }

    public async Task<IdPCommandResult<IdPOidcApplication>> CreateAsync(IdPCommand<IdPOidcApplication> command, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var provider = await CreateProviderAsync(command, cancellationToken);
        
        var application = await applicationHttpClient.CreateAsync(
            BuildApplication(command.Entity, provider!.pk, null),
            command.Entity.IdPProvider,
            cancellationToken);

        logger.LogInformation("Created Authentik OIDC application {DisplayName} with application id {ApplicationId}", application.name, application.slug);

        return Result(command, application, provider, provider.client_secret, now);
    }

    public async Task<AuthentikOAuth2ProviderV3> CreateProviderAsync(
        IdPCommand<IdPOidcApplication> command,
        CancellationToken cancellationToken)
    {
        var clientId = GenerateToken(24);
        var clientSecret = command.Entity.Spec.UsesClientSecret ? GenerateToken(48) : string.Empty;
        
        var provider = await providerHttpClient.CreateAsync(
            await BuildProviderAsync(command.Entity, clientId, clientSecret, null, cancellationToken),
            command.Entity.IdPProvider,
            cancellationToken);
        
        return provider;
    }

    public async Task<IdPCommandResult<IdPOidcApplication>> UpdateAsync(
        AuthentikOidcApplicationV3 current,
        IdPCommand<IdPOidcApplication> command,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var clientId = string.IsNullOrEmpty(command.Entity.Status.ClientId)
            ? current.Provider?.client_id ?? GenerateToken(24)
            : command.Entity.Status.ClientId;
        var usesClientSecret = command.Entity.Spec.UsesClientSecret;
        var shouldRotate = usesClientSecret &&
                           (command.Entity.Spec.RotateClientSecret || string.IsNullOrEmpty(command.Entity.ClientSecret));
        var clientSecret = usesClientSecret
            ? shouldRotate ? GenerateToken(48) : command.Entity.ClientSecret
            : string.Empty;

        var desiredProvider = await BuildProviderAsync(
            command.Entity,
            clientId,
            clientSecret,
            current.Application.provider,
            cancellationToken);
        var updatedProvider = await ReconcileProviderAsync(
            current.Provider!,
            desiredProvider,
            shouldRotate,
            command,
            cancellationToken);

        var desiredApplication = BuildApplication(command.Entity, updatedProvider.pk ?? current.Application.provider, current.Application.pk);
        var updatedApplication = await ReconcileApplicationAsync(
            current.Application,
            desiredApplication,
            command,
            cancellationToken);
        
        return Result(command, updatedApplication, updatedProvider, clientSecret, shouldRotate ? now : command.Entity.Status.LastRotatedAt ?? now);
    }

    public async Task<bool> DeleteAsync(IdPCommand<IdPOidcApplication> command, CancellationToken cancellationToken)
    {
        if (!command.Entity.Spec.AutoDelete)
        {
            logger.LogInformation("Auto delete is disabled, skipping OIDC application deletion for application id {ApplicationId}", command.Entity.Status.ApplicationId);
            return false;
        }

        var oidcApplication = await GetAsync(command, cancellationToken);
        if (oidcApplication is null)
        {
            logger.LogInformation("OIDC application does not exist in Authentik, skipping deletion");
            return false;
        }

        var deletedApplication = await applicationHttpClient.DeleteAsync(
            oidcApplication.Application.slug,
            command.Entity.IdPProvider,
            cancellationToken);

        var deletedProvider = oidcApplication.Provider is null ||
                              await providerHttpClient.DeleteAsync(
                                  oidcApplication.Provider.pk!.Value,
                                  command.Entity.IdPProvider,
                                  cancellationToken);

        logger.LogInformation(
            "Delete request for OIDC application id {ApplicationId} completed with application deleted {ApplicationDeleted} and provider deleted {ProviderDeleted}",
            oidcApplication.Application.slug,
            deletedApplication,
            deletedProvider);

        return deletedApplication && deletedProvider;
    }

    private async Task<AuthentikApplicationV3?> GetApplicationAsync(
        IdPCommand<IdPOidcApplication> command,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(command.Entity.Status.ApplicationId))
        {
            logger.LogDebug("Looking up Authentik application by application id {ApplicationId}", command.Entity.Status.ApplicationId);
            return await applicationHttpClient.GetByIdAsync(
                command.Entity.Status.ApplicationId,
                command.Entity.IdPProvider,
                null,
                cancellationToken);
        }

        logger.LogDebug("Looking up Authentik application by display name {DisplayName}", command.Entity.Spec.DisplayName);
        return await applicationHttpClient.GetByNameAsync(
            command.Entity.Spec.DisplayName,
            command.Entity.IdPProvider,
            null,
            cancellationToken);
    }

    private async Task<AuthentikOAuth2ProviderV3> ReconcileProviderAsync(
        AuthentikOAuth2ProviderV3 current,
        AuthentikOAuth2ProviderV3 desired,
        bool shouldRotate,
        IdPCommand<IdPOidcApplication> command,
        CancellationToken cancellationToken)
    {
        if (!ShouldUpdate(current, desired, shouldRotate))
        {
            logger.LogInformation("OIDC provider {ProviderName} is already up to date with provider id {ProviderId}", current.name, current.pk);
            return current;
        }

        logger.LogInformation("OIDC provider {ProviderName} is not up to date, updating provider id {ProviderId}", desired.name, current.pk);
        return await providerHttpClient.UpdateAsync(current.pk!.Value, desired, command.Entity.IdPProvider, cancellationToken);
    }

    private async Task<AuthentikApplicationV3> ReconcileApplicationAsync(
        AuthentikApplicationV3 current,
        AuthentikApplicationV3 desired,
        IdPCommand<IdPOidcApplication> command,
        CancellationToken cancellationToken)
    {
        if (!ShouldUpdate(current, desired))
        {
            logger.LogInformation("OIDC application {DisplayName} is already up to date with application id {ApplicationId}", current.name, current.slug);
            return current;
        }

        logger.LogInformation("OIDC application {DisplayName} is not up to date, updating application id {ApplicationId}", desired.name, current.slug);
        return await applicationHttpClient.UpdateAsync(current.slug, desired, command.Entity.IdPProvider, cancellationToken);
    }

    private async Task<AuthentikOAuth2ProviderV3> BuildProviderAsync(
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

        var authorizationFlowId = await GetFlowIdAsync(
            application.IdPProvider,
            authorizationFlowSlug,
            cancellationToken);
        var invalidationFlowId = await GetFlowIdAsync(
            application.IdPProvider,
            invalidationFlowSlug,
            cancellationToken);

        return new AuthentikOAuth2ProviderV3
        {
            name = application.Spec.DisplayName,
            pk = providerId,
            client_type = application.Spec.UsesClientSecret ? "confidential" : "public",
            client_id = clientId,
            client_secret = application.Spec.UsesClientSecret ? clientSecret : string.Empty,
            authorization_flow = authorizationFlowId,
            invalidation_flow = invalidationFlowId,
            attributes = DefaultAttributes,
            redirect_uris = application.Spec.RedirectUris
                .Select(uri => new AuthentikRedirectUriV3(redirectUriMatchingMode, uri))
                .Concat(application.Spec.PostLogoutRedirectUris.Select(
                    uri => new AuthentikRedirectUriV3(redirectUriMatchingMode, uri, "post_logout")))
        };
    }

    private async Task<string> GetFlowIdAsync(
        IdPProvider provider,
        string flowSlug,
        CancellationToken cancellationToken)
    {
        var flow = await flowHttpClient.GetByIdAsync(flowSlug, provider, null, cancellationToken);

        if (flow is null || string.IsNullOrWhiteSpace(flow.pk))
        {
            throw new IdPException(
                $"Authentik flow \"{flowSlug}\" was not found.",
                IdPExceptionReason.IdpValidationFailed);
        }

        return flow.pk;
    }

    private static AuthentikApplicationV3 BuildApplication(
        IdPOidcApplication application,
        int? providerId,
        string? applicationPk)
    {
        return new AuthentikApplicationV3(
            application.Spec.DisplayName,
            Slug(application.Name),
            applicationPk,
            providerId,
            string.IsNullOrWhiteSpace(application.Spec.LaunchUrl) ? null : application.Spec.LaunchUrl);
    }

    private static bool ShouldUpdate(
        AuthentikOAuth2ProviderV3 current,
        AuthentikOAuth2ProviderV3 desired,
        bool shouldRotate)
    {
        if (!desired.name.Equals(current.name))
            return true;

        if (!desired.client_id.Equals(current.client_id))
            return true;

        if (!desired.client_type.Equals(current.client_type))
            return true;

        if (shouldRotate)
            return true;

        if (!string.Equals(desired.authorization_flow, current.authorization_flow, StringComparison.Ordinal))
            return true;

        if (!string.Equals(desired.invalidation_flow, current.invalidation_flow, StringComparison.Ordinal))
            return true;

        var desiredRedirectUris = desired.redirect_uris
            .Select(uri => (uri.matching_mode, uri.url, uri.redirect_uri_type))
            .ToHashSet();
        var currentRedirectUris = current.redirect_uris
            .Select(uri => (uri.matching_mode, uri.url, uri.redirect_uri_type))
            .ToHashSet();

        return !desiredRedirectUris.SetEquals(currentRedirectUris);
    }

    private static void ValidateSupportedCoreProperties(IdPOidcApplication application)
    {
        if (!application.Spec.Scopes.SequenceEqual(new[] { "openid" }, StringComparer.OrdinalIgnoreCase))
        {
            throw new IdPException(
                "Authentik OIDC application support currently only reconciles the default \"openid\" scope.",
                IdPExceptionReason.IdpValidationFailed);
        }

        if (!application.Spec.GrantTypes.SequenceEqual(new[] { "authorization_code" }, StringComparer.OrdinalIgnoreCase))
        {
            throw new IdPException(
                "Authentik OIDC application support currently only reconciles the \"authorization_code\" grant type.",
                IdPExceptionReason.IdpValidationFailed);
        }

        if (!application.Spec.ResponseTypes.SequenceEqual(new[] { "code" }, StringComparer.OrdinalIgnoreCase))
        {
            throw new IdPException(
                "Authentik OIDC application support currently only reconciles the \"code\" response type.",
                IdPExceptionReason.IdpValidationFailed);
        }

        if (application.Spec.ClientAuthenticationMethod == OidcClientAuthenticationMethod.ClientSecretPost)
        {
            throw new IdPException(
                "Authentik OIDC application support currently does not reconcile the \"client_secret_post\" client authentication method.",
                IdPExceptionReason.IdpValidationFailed);
        }

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

    private static bool ShouldUpdate(AuthentikApplicationV3 current, AuthentikApplicationV3 desired)
    {
        return !desired.name.Equals(current.name)
               || !desired.slug.Equals(current.slug)
               || !desired.provider.Equals(current.provider)
               || !string.Equals(desired.launch_url, current.launch_url, StringComparison.Ordinal);
    }

    private static IdPCommandResult<IdPOidcApplication> Result(
        IdPCommand<IdPOidcApplication> command,
        AuthentikApplicationV3 application,
        AuthentikOAuth2ProviderV3 provider,
        string clientSecret,
        DateTime lastRotatedAt)
    {
        return new IdPCommandResult<IdPOidcApplication>(
            command.Id,
            command.Entity.CopyWithNewStatus(
                new IdPOidcApplicationStatus(
                    application.slug,
                    provider.client_id,
                    command.Entity.Spec.UsesClientSecret ? lastRotatedAt : null,
                    command.Entity.Spec.UsesClientSecret ? lastRotatedAt.AddDays(command.Entity.Spec.RotationDays) : null,
                    new Dictionary<string, string>
                    {
                        ["authentikApplicationSlug"] = application.slug,
                        ["authentikOAuth2ProviderId"] = provider.pk?.ToString() ?? string.Empty
                    }),
                clientSecret));
    }

    private static string GenerateToken(int byteCount)
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(byteCount))
            .Replace("+", string.Empty)
            .Replace("/", string.Empty)
            .Replace("=", string.Empty);
    }

    private static string Slug(string value)
    {
        var slug = SlugRegex().Replace(value.ToLowerInvariant(), "-").Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? "oidc-application" : slug;
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex SlugRegex();
}
