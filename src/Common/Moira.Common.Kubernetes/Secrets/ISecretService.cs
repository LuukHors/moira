using Moira.Common.Kubernetes.Secrets.Models;

namespace Moira.Common.Kubernetes.Secrets;

public interface ISecretService
{
    Task<SecretStatus> SyncAsync(Secret target, CancellationToken cancellationToken);
    Task DeleteAsync(string secretName, string secretNamespace, ClusterSecretRef? clusterRef, CancellationToken cancellationToken);
}
