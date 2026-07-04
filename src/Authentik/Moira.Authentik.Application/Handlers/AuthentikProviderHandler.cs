using Moira.Authentik.Application.Ports;
using Moira.Common.Abstractions.Commands;
using Moira.Common.Abstractions.Exceptions;
using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Application.Handlers;

public class AuthentikProviderHandler(
    IAuthentikProviderCheckService providerCheckService) : IAuthentikProviderHandler
{
    public async Task CheckAsync(IdPCommand<IdPProvider> command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Entity.BaseUrl))
            throw new IdPException("Provider baseUrl should not be empty.", IdPExceptionReason.IdpValidationFailed);

        await providerCheckService.CheckHealthAsync(command.Entity, cancellationToken);
        await providerCheckService.CheckAuthenticatedAsync(command.Entity, cancellationToken);
    }

    public async Task<bool> DeleteAsync(IdPCommand<IdPProvider> command, CancellationToken cancellationToken)
    {
        await providerCheckService.ReleaseAsync(command.Entity, cancellationToken);
        return true;
    }
}