using Moira.KubeOps.Secrets.Models;

namespace Moira.KubeOps.Secrets;

public interface ISecretService
{
    Task<SecretTargetStatus> SyncAsync(SecretTarget target, CancellationToken cancellationToken);
    Task DeleteAsync(SecretTarget target, CancellationToken cancellationToken);
}
