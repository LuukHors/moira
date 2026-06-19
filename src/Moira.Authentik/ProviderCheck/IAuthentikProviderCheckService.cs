using Moira.Common.Models;

namespace Moira.Authentik.ProviderCheck;

public interface IAuthentikProviderCheckService
{
    Task CheckAsync(IdPProvider provider, CancellationToken cancellationToken);
}
