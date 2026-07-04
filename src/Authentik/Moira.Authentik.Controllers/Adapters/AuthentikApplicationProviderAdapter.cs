using Moira.Authentik.Application.Models;
using Microsoft.Extensions.Logging;
using Moira.Authentik.Application.Handlers;
using Moira.Authentik.Domain.Applications;
using Moira.Common.Abstractions;
using Moira.Common.Abstractions.Commands;
using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Controllers.Adapters;

public class AuthentikApplicationProviderAdapter(
    IAuthentikOidcApplicationHandler handler,
    ILogger<AuthentikApplicationProviderAdapter> logger) : AbstractAuthentikProviderAdapter, IProviderAdapter<AuthentikOidcApplicationModel>
{
    public async Task<IdPCommandResult<AuthentikOidcApplicationModel>> ExecuteReconcileAsync(IdPCommand<AuthentikOidcApplicationModel> command, CancellationToken cancellationToken)
    {
        logger.LogDebug("Reconciling Authentik OIDC application {ApplicationName}", command.Entity.Name);
        var oidcApplication = await handler.GetAsync(command, cancellationToken);

        if (oidcApplication is null)
        {
            return await handler.CreateAsync(command, cancellationToken);
        }

        if (oidcApplication.Provider is null)
        {
            var provider = await handler.CreateProviderAsync(command, cancellationToken);
            return await handler.UpdateAsync(oidcApplication with { Provider = provider }, command, cancellationToken);
        }

        return await handler.UpdateAsync(oidcApplication, command, cancellationToken);
    }

    public Task<bool> ExecuteDeleteAsync(IdPCommand<AuthentikOidcApplicationModel> command, CancellationToken cancellationToken)
    {
        logger.LogDebug("Deleting Authentik OIDC application {ApplicationName}", command.Entity.Name);
        return handler.DeleteAsync(command, cancellationToken);
    }
}
