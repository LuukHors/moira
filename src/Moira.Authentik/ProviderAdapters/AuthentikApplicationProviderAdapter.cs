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

        if (oidcApplication is null) //both application and provider do not exist.
        {
            return await handler.CreateAsync(command, cancellationToken);
        }

        if (oidcApplication.Provider is null) //application has been set, provider does not exist.
        {
            var provider = await handler.CreateProviderAsync(command, cancellationToken);
            return await handler.UpdateAsync(oidcApplication with { Provider = provider }, command, cancellationToken);
        }

        return await handler.UpdateAsync(oidcApplication, command, cancellationToken);
    }

    public Task<bool> ExecuteDeleteAsync(IdPCommand<IdPOidcApplication> command, CancellationToken cancellationToken)
    {
        logger.LogDebug("Deleting Authentik OIDC application {ApplicationName}", command.Entity.Name);
        return handler.DeleteAsync(command, cancellationToken);
    }
}
