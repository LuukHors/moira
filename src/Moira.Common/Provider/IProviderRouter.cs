using Moira.Common.Models;

namespace Moira.Common.Provider;

public interface IProviderRouter<TEntity> where TEntity : IdPEntity
{
    public Task<IProviderAdapter<TEntity>> ResolveProviderAsync(string providerName, CancellationToken cancellationToken);
}