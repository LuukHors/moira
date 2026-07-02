using KubeOps.Abstractions.Entities;
using Moira.Common.Abstractions.Models;

namespace Moira.Common.KubeOps.DependencyProvider;

public interface IDependencyProvider<in TK8SEntity, TResult> where TK8SEntity : CustomKubernetesEntity where TResult : IdPEntityBase
{
    Task<TResult> ResolveAsync(TK8SEntity entity, CancellationToken cancellationToken);
}
