using Moira.Common.KubeOps.Secrets.Models;

namespace Moira.Common.KubeOps.Secrets;

public interface ISecretService
{
    Task<SecretTargetStatus> SyncAsync(SecretTarget target, CancellationToken cancellationToken);
    Task DeleteAsync(SecretTarget target, CancellationToken cancellationToken);
}
