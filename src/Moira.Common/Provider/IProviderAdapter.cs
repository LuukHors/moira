using Moira.Common.Commands;
using Moira.Common.Models;

namespace Moira.Common.Provider;

public interface IProviderAdapter<TEntity> where TEntity : IdPEntity
{
    public string Name { get; }
    public Task<IdPCommandResult> ExecuteAsync(IdPCommand<TEntity> command);
}