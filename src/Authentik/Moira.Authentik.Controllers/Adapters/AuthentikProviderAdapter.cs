using Microsoft.Extensions.Logging;
using Moira.Authentik.Application.Ports;
using Moira.Authentik.Infrastructure.Authentication;
using Moira.Common.Commands;
using Moira.Common.Models;
using Moira.Common.Provider;

namespace Moira.Authentik.Controllers.Adapters;

public class AuthentikProviderAdapter(
    IAuthentikAuthenticationService authenticationService,
    IAuthentikProviderCheckService providerCheckService,
    ILogger<AuthentikProviderAdapter> logger) : AbstractAuthentikProviderAdapter, IProviderAdapter<IdPProvider>
{
    public async Task<IdPCommandResult<IdPProvider>> ExecuteReconcileAsync(IdPCommand<IdPProvider> command, CancellationToken cancellationToken)
    {
        logger.LogDebug("Validating Authentik provider {ProviderName}", command.Entity.Name);
        await providerCheckService.CheckAsync(command.Entity, cancellationToken);
        logger.LogDebug("Validated Authentik provider {ProviderName}", command.Entity.Name);
        return new IdPCommandResult<IdPProvider>(command.Id, command.Entity);
    }

    public Task<bool> ExecuteDeleteAsync(IdPCommand<IdPProvider> command, CancellationToken cancellationToken)
    {
        var tokenInvalidated = authenticationService.InvalidateCachedToken(command.Entity.Name);
        logger.LogInformation("Invalidated cached Authentik token for provider {ProviderName}: {TokenInvalidated}", command.Entity.Name, tokenInvalidated);
        return Task.FromResult(true);
    }
}
