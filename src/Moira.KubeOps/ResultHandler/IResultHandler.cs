using KubeOps.Abstractions.Entities;
using Moira.Common.Exceptions;
using Moira.Common.Models;

namespace Moira.KubeOps.ResultHandler;

public interface IResultHandler<in TK8SEntity, in TEntity> where TK8SEntity : CustomKubernetesEntity where TEntity : IdPEntity
{
    Task HandleAsync(TK8SEntity entity, CancellationToken cancellationToken, TEntity? idpEntity = null, IdPException? exception = null);
}