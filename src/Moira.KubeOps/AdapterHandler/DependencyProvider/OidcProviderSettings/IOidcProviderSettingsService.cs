using Moira.Common.Models;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.AdapterHandler.DependencyProvider.OidcProviderSettings;

public interface IOidcProviderSettingsService
{
    Task<Moira.Common.Models.OidcProviderSettings?> ResolveAsync(
        OidcApplication entity,
        IdPProvider provider,
        CancellationToken cancellationToken);
}
