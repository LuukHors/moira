using Moira.Common.Abstractions.Models;

namespace Moira.Common.Abstractions;

public interface IProviderRouter<TEntity> where TEntity : IdPEntityBase
{
    public Task<IProviderAdapter<TEntity>> ResolveAsync(string providerName, CancellationToken cancellationToken);
}
