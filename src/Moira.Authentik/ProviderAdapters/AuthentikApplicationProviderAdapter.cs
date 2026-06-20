using Microsoft.Extensions.Logging;
using Moira.Authentik.Handlers;
using Moira.Authentik.Models.V3;
using Moira.Common.Commands;
using Moira.Common.Models;
using Moira.Common.Provider;

namespace Moira.Authentik.ProviderAdapters;

public class AuthentikApplicationProviderAdapter(
    IAuthentikHandler<IdPOidcApplication, AuthentikOidcApplicationV3> handler,
    ILogger<AuthentikApplicationProviderAdapter> logger) : AbstractAuthentikProviderAdapter, IProviderAdapter<IdPOidcApplication>
{
    public async Task<IdPCommandResult<IdPOidcApplication>> ExecuteReconcileAsync(IdPCommand<IdPOidcApplication> command, CancellationToken cancellationToken)
    {
        logger.LogDebug("Reconciling Authentik OIDC application {ApplicationName}", command.Entity.Name);
        var oidcApplication = await handler.GetAsync(command, cancellationToken);

        if (oidcApplication is null)
        {
            logger.LogDebug("Creating Authentik OIDC application {ApplicationName}", command.Entity.Name);
            return await handler.CreateAsync(command, cancellationToken);
        }

        logger.LogDebug("Updating Authentik OIDC application {ApplicationName}", command.Entity.Name);
        return await handler.UpdateAsync(oidcApplication, command, cancellationToken);
    }

    public Task<bool> ExecuteDeleteAsync(IdPCommand<IdPOidcApplication> command, CancellationToken cancellationToken)
    {
        logger.LogDebug("Deleting Authentik OIDC application {ApplicationName}", command.Entity.Name);
        return handler.DeleteAsync(command, cancellationToken);
    }
}
