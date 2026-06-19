using Moira.Authentik.Authentication;
using Microsoft.Extensions.Logging;
using Moira.Common.Commands;
using Moira.Common.Exceptions;
using Moira.Common.Models;
using Moira.Common.Provider;

namespace Moira.Authentik.ProviderAdapters;

public class AuthentikProviderAdapter(
    IAuthentikAuthenticationService authenticationService,
    ILogger<AuthentikProviderAdapter> logger) : AbstractAuthentikProviderAdapter, IProviderAdapter<IdPProvider>
{
    public async Task<IdPCommandResult<IdPProvider>> ExecuteReconcileAsync(IdPCommand<IdPProvider> command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Entity.BaseUrl))
        {
            throw new IdPException("Provider baseUrl should not be empty.", IdPExceptionReason.IdpValidationFailed);
        }

        logger.LogDebug("Validating Authentik provider {ProviderName}", command.Entity.Name);
        await authenticationService.AcquireTokenAsync(command.Entity, cancellationToken);
        logger.LogInformation("Authentik provider {ProviderName} was validated", command.Entity.Name);
        return new IdPCommandResult<IdPProvider>(command.Id, command.Entity);
    }

    public Task<bool> ExecuteDeleteAsync(IdPCommand<IdPProvider> command, CancellationToken cancellationToken)
    {
        var tokenInvalidated = authenticationService.InvalidateCachedToken(command.Entity.Name);
        logger.LogInformation("Invalidated cached Authentik token for provider {ProviderName}: {TokenInvalidated}", command.Entity.Name, tokenInvalidated);
        return Task.FromResult(true);
    }
}
