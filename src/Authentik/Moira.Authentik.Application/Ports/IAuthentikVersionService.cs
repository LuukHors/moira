using Moira.Authentik.Domain;
using Moira.Common.Models;

namespace Moira.Authentik.Application.Ports;

public interface IAuthentikVersionService
{
    Task<AuthentikVersion> GetVersionAsync(IdPProvider provider, CancellationToken cancellationToken);
}
