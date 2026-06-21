using Microsoft.Extensions.Logging;
using Moira.Authentik.Handlers;
using Moira.Authentik.Models.V3;
using Moira.Common.Commands;
using Moira.Common.Models;
using Moira.Common.Provider;

namespace Moira.Authentik.ProviderAdapters;

public class AuthentikApplicationProviderAdapter(
    IAuthentikOidcApplicationHandler handler,
    ILogger<AuthentikApplicationProviderAdapter> logger) : AbstractAuthentikProviderAdapter, IProviderAdapter<IdPOidcApplication>
{
    public async Task<IdPCommandResult<IdPOidcApplication>> ExecuteReconcileAsync(IdPCommand<IdPOidcApplication> command, CancellationToken cancellationToken)
    {
        logger.LogDebug("Reconciling Authentik OIDC application {ApplicationName}", command.Entity.Name);
        var oidcApplication = await handler.GetAsync(command, cancellationToken);
        var linkedProvider = oidcApplication?.Provider is not null && IsManagedByMoira(oidcApplication.Provider)
            ? oidcApplication.Provider
            : null;
        var provider = linkedProvider ?? await handler.GetProviderAsync(command, cancellationToken);

        if (oidcApplication is null)
        {
            if (provider is null)
            {
                logger.LogDebug("Creating Authentik OIDC application {ApplicationName} and provider", command.Entity.Name);
                return await handler.CreateAsync(command, cancellationToken);
            }

            logger.LogDebug(
                "Creating Authentik OIDC application {ApplicationName} for existing provider {ProviderId}",
                command.Entity.Name,
                provider.pk);
            var application = await handler.CreateApplicationAsync(command, provider, cancellationToken);
            return await handler.UpdateAsync(new AuthentikOidcApplicationV3(application, provider), command, cancellationToken);
        }

        logger.LogDebug("Updating Authentik OIDC application {ApplicationName}", command.Entity.Name);
        var applicationForUpdate = provider is null
            ? oidcApplication.Application with { provider = null }
            : oidcApplication.Application;

        return await handler.UpdateAsync(
            oidcApplication with { Application = applicationForUpdate, Provider = provider },
            command,
            cancellationToken);
    }

    public Task<bool> ExecuteDeleteAsync(IdPCommand<IdPOidcApplication> command, CancellationToken cancellationToken)
    {
        logger.LogDebug("Deleting Authentik OIDC application {ApplicationName}", command.Entity.Name);
        return handler.DeleteAsync(command, cancellationToken);
    }

    private static bool IsManagedByMoira(AuthentikOAuth2ProviderV3 provider)
    {
        return provider.attributes.TryGetValue("managed-by", out var managedBy)
               && managedBy?.ToString() == "moira";
    }
}
