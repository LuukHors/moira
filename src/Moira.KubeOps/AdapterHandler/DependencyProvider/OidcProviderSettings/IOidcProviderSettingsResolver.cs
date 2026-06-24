using Moira.Common.Models;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.AdapterHandler.DependencyProvider.OidcProviderSettings;

public interface IOidcProviderSettingsResolver
{
    bool CanResolve(IdPProvider provider, ResourceRef settingsRef);

    Task<Moira.Common.Models.OidcProviderSettings> ResolveAsync(
        ResourceRef settingsRef,
        string defaultNamespace,
        IdPProvider provider,
        CancellationToken cancellationToken);
}
