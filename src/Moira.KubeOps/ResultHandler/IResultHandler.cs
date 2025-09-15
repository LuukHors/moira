using Moira.Common.Commands;
using Moira.Common.Models;

namespace Moira.KubeOps.ResultHandler;

public interface IResultHandler<TEntity> where TEntity : IdPEntity
{
    Task HandleAsync(IdPCommandResult<TEntity> result, CancellationToken cancellationToken);
}