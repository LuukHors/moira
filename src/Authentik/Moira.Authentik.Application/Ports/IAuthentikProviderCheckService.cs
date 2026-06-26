using Moira.Common.Models;

namespace Moira.Authentik.Application.Ports;

public interface IAuthentikProviderCheckService
{
    Task CheckAsync(IdPProvider provider, CancellationToken cancellationToken);
}
