using Moira.Common.Abstractions.Models;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.Secrets;

public interface IOidcApplicationSecretService
{
    Task<IEnumerable<OidcApplication.SecretStatus>> SyncAsync(
        OidcApplication entity,
        IdPOidcApplication idpEntity,
        CancellationToken cancellationToken);

    Task DeleteAsync(OidcApplication entity, CancellationToken cancellationToken);
}
