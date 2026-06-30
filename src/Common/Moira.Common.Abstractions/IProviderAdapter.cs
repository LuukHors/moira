using Moira.Common.Abstractions.Commands;
using Moira.Common.Abstractions.Models;

namespace Moira.Common.Abstractions;

public interface IProviderAdapter<TEntity> where TEntity : IdPEntityBase
{
    public string Name { get; }
    public Task<IdPCommandResult<TEntity>> ExecuteReconcileAsync(IdPCommand<TEntity> command, CancellationToken cancellationToken);
    public Task<bool> ExecuteDeleteAsync(IdPCommand<TEntity> command, CancellationToken cancellationToken);
}
