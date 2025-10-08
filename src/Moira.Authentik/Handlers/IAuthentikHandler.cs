using Moira.Common.Commands;
using Moira.Common.Models;

namespace Moira.Authentik.Handlers;

public interface IAuthentikHandler<TEntity, TAuthentikModel> where TEntity : IdPEntity
{
    Task<TAuthentikModel?> GetAsync(IdPCommand<TEntity> command, CancellationToken cancellationToken);
    Task<IdPCommandResult<TEntity>> CreateAsync(IdPCommand<TEntity> command, CancellationToken cancellationToken);
    Task<IdPCommandResult<TEntity>> UpdateAsync(TAuthentikModel currentEntity, IdPCommand<TEntity> command, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(IdPCommand<TEntity> command, CancellationToken cancellationToken);
}