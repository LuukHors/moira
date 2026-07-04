using Microsoft.Extensions.Logging;
using Moira.Common.Abstractions;
using Moira.Common.Abstractions.Exceptions;
using Moira.Common.Abstractions.Models;

namespace Moira.Common.Services.Provider;

public class ProviderRouter<TEntity> (
    IEnumerable<IProviderAdapter<TEntity>> adapters,
    ILogger<ProviderRouter<TEntity>> logger) : IProviderRouter<TEntity> where TEntity : IdPEntityBase
{
    public Task<IProviderAdapter<TEntity>> ResolveAsync(string providerName, CancellationToken cancellationToken)
    {
        logger.LogDebug("Resolving provider adapter {ProviderName}", providerName);
        var adapter = adapters.FirstOrDefault(
            provider =>
                provider.Name.Equals(providerName, StringComparison.OrdinalIgnoreCase));

        if (adapter is null) throw new ProviderAdapterNotFoundException(providerName);
        
        logger.LogDebug("Resolved provider adapter {ProviderName}", providerName);
        return Task.FromResult(adapter);
    }
}
