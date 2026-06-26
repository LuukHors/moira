using Moira.Common.Exceptions;
using Moira.Common.Models;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.AdapterHandler.DependencyProvider.OidcProviderSettings;

public class OidcProviderSettingsService(
    IEnumerable<IOidcProviderSettingsResolver> resolvers) : IOidcProviderSettingsService
{
    public async Task<Moira.Common.Models.OidcProviderSettings?> ResolveAsync(
        OidcApplication entity,
        IdPProvider provider,
        CancellationToken cancellationToken)
    {
        var settingsRef = entity.Spec.ProviderSettingsRef;
        if (settingsRef is null)
        {
            return null;
        }

        var resolver = resolvers.FirstOrDefault(r => r.CanResolve(provider, settingsRef));
        if (resolver is null)
        {
            throw new ProviderSettingsException(
                $"No OIDC provider settings resolver is registered for provider type \"{provider.Type}\" and settings kind \"{settingsRef.Kind}\".");
        }

        return await resolver.ResolveAsync(settingsRef, entity.Metadata.NamespaceProperty, provider, cancellationToken);
    }
}
