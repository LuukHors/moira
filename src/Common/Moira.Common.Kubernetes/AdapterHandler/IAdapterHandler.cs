using KubeOps.Abstractions.Entities;

namespace Moira.Common.Kubernetes.AdapterHandler;

public interface IAdapterHandler<in TK8SEntity> where TK8SEntity : CustomKubernetesEntity
{
    Task HandleReconcileAsync(TK8SEntity entity, CancellationToken cancellationToken);
    Task HandleDeleteAsync(TK8SEntity entity, CancellationToken cancellationToken);
}