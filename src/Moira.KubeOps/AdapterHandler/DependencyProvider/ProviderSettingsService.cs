using Moira.Common.Exceptions;
using Moira.Common.Models;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.AdapterHandler.DependencyProvider;

public class ProviderSettingsService<TEntity, TSettings>(
    IEnumerable<IProviderSettingsResolver<TSettings>> resolvers,
    Func<TEntity, ResourceRef?> getSettingsRef,
    Func<TEntity, string> getNamespace) : IProviderSettingsService<TEntity, TSettings>
{
    public async Task<TSettings?> ResolveAsync(TEntity entity, IdPProvider provider, CancellationToken cancellationToken)
    {
        var settingsRef = getSettingsRef(entity);
        if (settingsRef is null)
            return default;

        var resolver = resolvers.FirstOrDefault(r => r.CanResolve(provider, settingsRef))
            ?? throw new ProviderSettingsException(
                $"No provider settings resolver registered for provider type \"{provider.Type}\" and settings kind \"{settingsRef.Kind}\".");

        return await resolver.ResolveAsync(settingsRef, getNamespace(entity), provider, cancellationToken);
    }
}