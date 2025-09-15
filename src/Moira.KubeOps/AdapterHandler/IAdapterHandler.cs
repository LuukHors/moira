using KubeOps.Abstractions.Entities;
using Moira.Common.Models;

namespace Moira.KubeOps.AdapterHandler;

public interface IAdapterHandler<in TK8SEntity> where TK8SEntity : CustomKubernetesEntity
{
    Task HandleAsync(TK8SEntity entity, CancellationToken cancellationToken);
}