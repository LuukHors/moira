using k8s;
using k8s.Models;
using KubeOps.Abstractions.Entities;
using Moira.Common.Abstractions.Models;
using Moira.Common.Kubernetes.Secrets.Models;

namespace Moira.Common.Kubernetes.Secrets;

public interface IOidcApplicationSecretService<in TK8SEntity> where TK8SEntity : CustomKubernetesEntity
{
    Task<IEnumerable<SecretStatus>> SyncAsync(
        TK8SEntity entity,
        OidcApplicationData data,
        IEnumerable<Secret> desiredSecrets,
        IEnumerable<SecretStatus> previousStatuses,
        CancellationToken cancellationToken);

    Task DeleteAsync(TK8SEntity entity, IEnumerable<Secret> secrets, CancellationToken cancellationToken);
}
