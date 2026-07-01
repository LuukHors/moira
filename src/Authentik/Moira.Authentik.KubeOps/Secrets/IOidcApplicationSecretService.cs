using Moira.Authentik.Application.Models;
using Moira.Common.Abstractions.Models;
using Moira.Authentik.KubeOps.Entities;

namespace Moira.Authentik.KubeOps.Secrets;

public interface IOidcApplicationSecretService
{
    Task<IEnumerable<AuthentikOidcApplication.SecretStatus>> SyncAsync(
        AuthentikOidcApplication entity,
        AuthentikOidcApplicationModel idpEntity,
        CancellationToken cancellationToken);

    Task DeleteAsync(AuthentikOidcApplication entity, CancellationToken cancellationToken);
}
