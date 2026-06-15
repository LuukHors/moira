using KubeOps.Abstractions.Entities;
using Moira.Common.Models;

namespace Moira.KubeOps.DependencyProvider;

public interface IDependencyProvider<in TK8SEntity, TResult> where TK8SEntity : CustomKubernetesEntity where TResult : IdPEntityBase
{
    Task<TResult> ResolveAsync(TK8SEntity entity, CancellationToken cancellationToken);
}