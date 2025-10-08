using KubeOps.Abstractions.Entities;
using Moira.Common.Exceptions;
using Moira.Common.Models;

namespace Moira.KubeOps.ResultHandler;

public interface IResultHandler<in TK8SEntity, in TEntity> where TK8SEntity : CustomKubernetesEntity where TEntity : IdPEntity
{
    Task HandleAsync(TK8SEntity entity, TEntity idpEntity, CancellationToken cancellationToken);
    Task HandleExceptionAsync(TK8SEntity entity, IdPException exception, CancellationToken cancellationToken);
}