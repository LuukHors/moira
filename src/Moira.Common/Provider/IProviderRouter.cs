using Moira.Common.Models;

namespace Moira.Common.Provider;

public interface IProviderRouter<TEntity> where TEntity : IdPEntityBase
{
    public Task<IProviderAdapter<TEntity>> ResolveAsync(string providerName, CancellationToken cancellationToken);
}
