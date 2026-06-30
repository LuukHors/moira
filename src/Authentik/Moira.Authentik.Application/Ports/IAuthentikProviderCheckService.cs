using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Application.Ports;

public interface IAuthentikProviderCheckService
{
    Task CheckHealthAsync(IdPProvider provider, CancellationToken cancellationToken);
    Task CheckAuthenticatedAsync(IdPProvider provider, CancellationToken cancellationToken);
    Task ReleaseAsync(IdPProvider provider, CancellationToken cancellationToken);
}
