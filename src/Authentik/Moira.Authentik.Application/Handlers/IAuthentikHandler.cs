using Moira.Common.Abstractions.Commands;
using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Application.Handlers;

public interface IAuthentikHandler<TEntity, TAuthentikModel> where TEntity : IdPEntity
{
    Task<TAuthentikModel?> GetAsync(IdPCommand<TEntity> command, CancellationToken cancellationToken);
    Task<IdPCommandResult<TEntity>> CreateAsync(IdPCommand<TEntity> command, CancellationToken cancellationToken);
    Task<IdPCommandResult<TEntity>> UpdateAsync(TAuthentikModel current, IdPCommand<TEntity> command, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(IdPCommand<TEntity> command, CancellationToken cancellationToken);
}
