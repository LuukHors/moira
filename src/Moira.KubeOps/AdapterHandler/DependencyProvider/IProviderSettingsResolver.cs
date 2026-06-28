using Moira.Common.Models;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.AdapterHandler.DependencyProvider;

public interface IProviderSettingsResolver<TSettings>
{
    bool CanResolve(IdPProvider provider, ResourceRef settingsRef);

    Task<TSettings> ResolveAsync(
        ResourceRef settingsRef,
        string defaultNamespace,
        IdPProvider provider,
        CancellationToken cancellationToken);
}