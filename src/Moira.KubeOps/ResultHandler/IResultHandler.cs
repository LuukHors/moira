using KubeOps.Abstractions.Entities;
using Moira.Common.Commands;
using Moira.Common.Models;

namespace Moira.KubeOps.ResultHandler;

public interface IResultHandler<TK8SEntity, TEntity> where TK8SEntity : CustomKubernetesEntity where TEntity : IdPEntity
{
    Task HandleAsync(TK8SEntity entity, IdPCommandResult<TEntity> result, CancellationToken cancellationToken);
}