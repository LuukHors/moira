using Moira.Common.Models;
using Microsoft.Extensions.Logging;

namespace Moira.Common.Provider;

public class ProviderRouter<TEntity> (
    IEnumerable<IProviderAdapter<TEntity>> adapters,
    ILogger<ProviderRouter<TEntity>> logger) : IProviderRouter<TEntity> where TEntity : IdPEntity 
{
    public async Task<IProviderAdapter<TEntity>> ResolveProviderAsync(string providerName, CancellationToken cancellationToken)
    {
        logger.LogDebug("Determining provider with name {providerName}", providerName);
        var adapter = adapters.FirstOrDefault(
            provider =>
                provider.Name.Equals(providerName, StringComparison.OrdinalIgnoreCase));
        
        logger.LogDebug("Found provider with name \"{providerName}\"", providerName);


        return adapter ?? throw new InvalidOperationException($"No adapter with name \"{providerName}\" found.");
    }
}