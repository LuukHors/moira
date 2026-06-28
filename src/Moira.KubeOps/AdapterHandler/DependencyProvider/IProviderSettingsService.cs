using Moira.Common.Models;

namespace Moira.KubeOps.AdapterHandler.DependencyProvider;

public interface IProviderSettingsService<TEntity, TSettings>
{
    Task<TSettings?> ResolveAsync(TEntity entity, IdPProvider provider, CancellationToken cancellationToken);
}