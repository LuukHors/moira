using KubeOps.Abstractions.Entities;
using Moira.Common.Abstractions.Exceptions;
using Moira.Common.Abstractions.Models;

namespace Moira.KubeOps.ResultHandler;

public interface IResultHandler<in TK8SEntity, in TEntity> where TK8SEntity : CustomKubernetesEntity where TEntity : IdPEntityBase
{
    Task HandleAsync(TK8SEntity entity, TEntity idpEntity, CancellationToken cancellationToken);
    Task HandleExceptionAsync(TK8SEntity entity, MoiraException exception, CancellationToken cancellationToken);
    Task HandleDeleteAsync(TK8SEntity entity, TEntity idpEntity, CancellationToken cancellationToken);
}
