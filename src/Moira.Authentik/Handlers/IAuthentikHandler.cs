using Moira.Common.Commands;
using Moira.Common.Models;

namespace Moira.Authentik.Handlers;

public interface IAuthentikHandler<TEntity, TAuthentikModel> where TEntity : IdPEntity
{
    Task<TAuthentikModel?> GetAsync(IdPCommand<TEntity> command);
    Task<IdPCommandResult<TEntity>> CreateAsync(IdPCommand<TEntity> command);
    Task<IdPCommandResult<TEntity>> UpdateAsync(IdPCommand<TEntity> command);
}