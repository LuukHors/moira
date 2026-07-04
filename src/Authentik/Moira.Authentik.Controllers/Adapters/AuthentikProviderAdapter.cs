using Microsoft.Extensions.Logging;
using Moira.Authentik.Application.Handlers;
using Moira.Common.Abstractions;
using Moira.Common.Abstractions.Commands;
using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Controllers.Adapters;

public class AuthentikProviderAdapter(
    IAuthentikProviderHandler providerHandler,
    ILogger<AuthentikProviderAdapter> logger) : AbstractAuthentikProviderAdapter, IProviderAdapter<IdPProvider>
{
    public async Task<IdPCommandResult<IdPProvider>> ExecuteReconcileAsync(IdPCommand<IdPProvider> command, CancellationToken cancellationToken)
    {
        logger.LogDebug("Validating Authentik provider {ProviderName}", command.Entity.Name);
        await providerHandler.CheckAsync(command, cancellationToken);
        logger.LogDebug("Validated Authentik provider {ProviderName}", command.Entity.Name);
        return new IdPCommandResult<IdPProvider>(command.Id, command.Entity);
    }

    public async Task<bool> ExecuteDeleteAsync(IdPCommand<IdPProvider> command, CancellationToken cancellationToken)
        => await providerHandler.DeleteAsync(command, cancellationToken);
}