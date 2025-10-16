using Moira.Common.Commands;
using Moira.Common.Models;

namespace Moira.Common.Provider;

public interface IProviderAdapter<TEntity> where TEntity : IdPEntity
{
    public string Name { get; }
    public Task<IdPCommandResult<TEntity>> ExecuteReconcileAsync(IdPCommand<TEntity> command, CancellationToken cancellationToken);
    public Task<bool> ExecuteDeleteAsync(IdPCommand<TEntity> command, CancellationToken cancellationToken);
}