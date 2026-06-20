using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Moira.Authentik.HttpService;
using Moira.Authentik.Models.V3;
using Moira.Common.Commands;
using Moira.Common.Models;

namespace Moira.Authentik.Handlers;

public partial class AuthentikOidcApplicationHandler(
    IHttpService<AuthentikApplicationV3, AuthentikApplicationV3, string> applicationHttpClient,
    IHttpService<AuthentikOAuth2ProviderV3, AuthentikOAuth2ProviderV3, string> providerHttpClient,
    ILogger<AuthentikOidcApplicationHandler> logger) : IAuthentikHandler<IdPOidcApplication, AuthentikOidcApplicationV3>
{
    public async Task<AuthentikOidcApplicationV3?> GetAsync(IdPCommand<IdPOidcApplication> command, CancellationToken cancellationToken)
    {
        var application = await GetApplicationAsync(command, cancellationToken);
        if (application is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(application.provider))
        {
            logger.LogInformation("Authentik application {ApplicationId} does not have an OAuth2 provider", application.pk);
            return new AuthentikOidcApplicationV3(application, null);
        }

        var provider = await providerHttpClient.GetByIdAsync(
            application.provider,
            command.Entity.IdPProvider,
            null,
            cancellationToken);

        if (provider is null)
        {
            logger.LogInformation("Authentik application {ApplicationId} references missing OAuth2 provider {ProviderId}", application.pk, application.provider);
        }

        return new AuthentikOidcApplicationV3(application, provider);
    }

    public async Task<IdPCommandResult<IdPOidcApplication>> CreateAsync(IdPCommand<IdPOidcApplication> command, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var clientId = GenerateToken(24);
        var clientSecret = GenerateToken(48);
        var provider = await providerHttpClient.CreateAsync(
            BuildProvider(command.Entity, clientId, clientSecret, null),
            command.Entity.IdPProvider,
            cancellationToken);

        var application = await applicationHttpClient.CreateAsync(
            BuildApplication(command.Entity, provider.pk ?? string.Empty, null),
            command.Entity.IdPProvider,
            cancellationToken);

        logger.LogInformation("Created Authentik OIDC application {DisplayName} with application id {ApplicationId}", application.name, application.pk);

        return Result(command, application, provider, clientSecret, now);
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
        var shouldRotate = command.Entity.Spec.RotateClientSecret || string.IsNullOrEmpty(command.Entity.ClientSecret);
        var clientSecret = shouldRotate ? GenerateToken(48) : command.Entity.ClientSecret;

        var desiredProvider = BuildProvider(command.Entity, clientId, clientSecret, current.Provider?.pk ?? current.Application.provider);
        var updatedProvider = await ReconcileProviderAsync(
            current.Provider,
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

        logger.LogInformation("Updated Authentik OIDC application {DisplayName} with application id {ApplicationId}", updatedApplication.name, updatedApplication.pk);

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
            logger.LogInformation("Complete OIDC application does not exist in Authentik, skipping deletion");
            return false;
        }

        var deletedApplication = await applicationHttpClient.DeleteAsync(
            oidcApplication.Application.pk!,
            command.Entity.IdPProvider,
            cancellationToken);

        var deletedProvider = oidcApplication.Provider is null ||
                              await providerHttpClient.DeleteAsync(
                                  oidcApplication.Provider.pk!,
                                  command.Entity.IdPProvider,
                                  cancellationToken);

        logger.LogInformation(
            "Delete request for OIDC application id {ApplicationId} completed with application deleted {ApplicationDeleted} and provider deleted {ProviderDeleted}",
            oidcApplication.Application.pk,
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
        AuthentikOAuth2ProviderV3? current,
        AuthentikOAuth2ProviderV3 desired,
        bool shouldRotate,
        IdPCommand<IdPOidcApplication> command,
        CancellationToken cancellationToken)
    {
        if (current is null)
        {
            logger.LogInformation("OIDC provider does not exist, creating provider {ProviderName}", desired.name);
            return await providerHttpClient.CreateAsync(desired, command.Entity.IdPProvider, cancellationToken);
        }

        if (!ShouldUpdate(current, desired, shouldRotate))
        {
            logger.LogInformation("OIDC provider {ProviderName} is already up to date with provider id {ProviderId}", current.name, current.pk);
            return current;
        }

        logger.LogInformation("OIDC provider {ProviderName} is not up to date, updating provider id {ProviderId}", desired.name, current.pk);
        return await providerHttpClient.UpdateAsync(current.pk!, desired, command.Entity.IdPProvider, cancellationToken);
    }

    private async Task<AuthentikApplicationV3> ReconcileApplicationAsync(
        AuthentikApplicationV3 current,
        AuthentikApplicationV3 desired,
        IdPCommand<IdPOidcApplication> command,
        CancellationToken cancellationToken)
    {
        if (!ShouldUpdate(current, desired))
        {
            logger.LogInformation("OIDC application {DisplayName} is already up to date with application id {ApplicationId}", current.name, current.pk);
            return current;
        }

        logger.LogInformation("OIDC application {DisplayName} is not up to date, updating application id {ApplicationId}", desired.name, current.pk);
        return await applicationHttpClient.UpdateAsync(current.pk!, desired, command.Entity.IdPProvider, cancellationToken);
    }

    private static AuthentikOAuth2ProviderV3 BuildProvider(
        IdPOidcApplication application,
        string clientId,
        string clientSecret,
        string? providerId)
    {
        return new AuthentikOAuth2ProviderV3
        {
            name = application.Spec.DisplayName,
            pk = providerId,
            client_id = clientId,
            client_secret = clientSecret,
            redirect_uris = application.Spec.RedirectUris
        };
    }

    private static AuthentikApplicationV3 BuildApplication(
        IdPOidcApplication application,
        string providerId,
        string? applicationId)
    {
        return new AuthentikApplicationV3(
            application.Spec.DisplayName,
            Slug(application.Name),
            applicationId,
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

        if (shouldRotate)
            return true;

        var desiredRedirectUris = desired.redirect_uris.ToHashSet();
        var currentRedirectUris = current.redirect_uris.ToHashSet();

        return !desiredRedirectUris.SetEquals(currentRedirectUris);
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
                    application.pk ?? string.Empty,
                    provider.client_id,
                    lastRotatedAt,
                    lastRotatedAt.AddDays(command.Entity.Spec.RotationDays)),
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
