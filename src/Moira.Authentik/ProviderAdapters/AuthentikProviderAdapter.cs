using Moira.Authentik.Authentication;
using Moira.Common.Commands;
using Moira.Common.Models;
using Moira.Common.Provider;

namespace Moira.Authentik.ProviderAdapters;

public class AuthentikProviderAdapter(
    IAuthentikAuthenticationService authenticationService) : AbstractAuthentikProviderAdapter, IProviderAdapter<IdPProvider>
{
    public async Task<IdPCommandResult<IdPProvider>> ExecuteReconcileAsync(IdPCommand<IdPProvider> command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Entity.BaseUrl))
        {
            throw new InvalidOperationException("Provider baseUrl should not be empty.");
        }

        await authenticationService.AcquireTokenAsync(command.Entity, cancellationToken);
        return new IdPCommandResult<IdPProvider>(command.Id, command.Entity);
    }

    public Task<bool> ExecuteDeleteAsync(IdPCommand<IdPProvider> command, CancellationToken cancellationToken)
    {
        authenticationService.InvalidateCachedToken(command.Entity.Name);
        return Task.FromResult(true);
    }
}
