using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Moira.Authentik.Application.Builders;
using Moira.Authentik.Application.Ports;
using Moira.Authentik.Domain.Applications;
using Moira.Common.Commands;
using Moira.Common.Models;
using Moira.Common.Provider;

namespace Moira.Authentik.Application.Handlers;

public partial class AuthentikOidcApplicationHandler(
    IAuthentikRepository<AuthentikApplicationV3, AuthentikApplicationV3, string> applicationRepository,
    IAuthentikRepository<AuthentikOAuth2ProviderV3, AuthentikOAuth2ProviderV3, int> providerRepository,
    IAuthentikOAuth2ProviderBuilder providerBuilder,
    IAuthentikApplicationBuilder applicationBuilder,
    IUpdateChecker<AuthentikOAuth2ProviderV3, AuthentikOAuth2ProviderV3> providerUpdateChecker,
    IUpdateChecker<AuthentikApplicationV3, AuthentikApplicationV3> applicationUpdateChecker,
    ILogger<AuthentikOidcApplicationHandler> logger) : IAuthentikOidcApplicationHandler
{
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

        var provider = await providerRepository.GetByIdAsync(
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

        var application = await applicationRepository.CreateAsync(
            applicationBuilder.Build(command.Entity, provider.pk, null),
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

        return await providerRepository.CreateAsync(
            await providerBuilder.BuildAsync(command.Entity, clientId, clientSecret, null, cancellationToken),
            command.Entity.IdPProvider,
            cancellationToken);
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

        var desiredProvider = await providerBuilder.BuildAsync(
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

        var desiredApplication = applicationBuilder.Build(command.Entity, updatedProvider.pk ?? current.Application.provider, current.Application.pk);
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

        var deletedApplication = await applicationRepository.DeleteAsync(
            oidcApplication.Application.slug,
            command.Entity.IdPProvider,
            cancellationToken);

        var deletedProvider = oidcApplication.Provider is null ||
                              await providerRepository.DeleteAsync(
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
            return await applicationRepository.GetByIdAsync(
                command.Entity.Status.ApplicationId,
                command.Entity.IdPProvider,
                null,
                cancellationToken);
        }

        logger.LogDebug("Looking up Authentik application by display name {DisplayName}", command.Entity.Spec.DisplayName);
        return await applicationRepository.GetByNameAsync(
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
        if (!shouldRotate && !providerUpdateChecker.ShouldUpdate(desired, current))
        {
            logger.LogInformation("OIDC provider {ProviderName} is already up to date with provider id {ProviderId}", current.name, current.pk);
            return current;
        }

        logger.LogInformation("OIDC provider {ProviderName} is not up to date, updating provider id {ProviderId}", desired.name, current.pk);
        return await providerRepository.UpdateAsync(current.pk!.Value, desired, command.Entity.IdPProvider, cancellationToken);
    }

    private async Task<AuthentikApplicationV3> ReconcileApplicationAsync(
        AuthentikApplicationV3 current,
        AuthentikApplicationV3 desired,
        IdPCommand<IdPOidcApplication> command,
        CancellationToken cancellationToken)
    {
        if (!applicationUpdateChecker.ShouldUpdate(desired, current))
        {
            logger.LogInformation("OIDC application {DisplayName} is already up to date with application id {ApplicationId}", current.name, current.slug);
            return current;
        }

        logger.LogInformation("OIDC application {DisplayName} is not up to date, updating application id {ApplicationId}", desired.name, current.slug);
        return await applicationRepository.UpdateAsync(current.slug, desired, command.Entity.IdPProvider, cancellationToken);
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
}