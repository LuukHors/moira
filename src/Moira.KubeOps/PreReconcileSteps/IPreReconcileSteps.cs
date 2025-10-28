using KubeOps.Abstractions.Entities;
using Moira.Common.Models;

namespace Moira.KubeOps.PreReconcileSteps;

public interface IPreReconcileSteps<in TK8SEntity> where TK8SEntity : CustomKubernetesEntity
{
    Task<bool> ExecuteAsync(TK8SEntity entity, CancellationToken cancellationToken);
}