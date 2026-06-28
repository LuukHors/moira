using Moira.Common.Models;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.AdapterHandler.DependencyProvider.OidcProviderSettings;

public class OidcProviderSettingsService(IEnumerable<IProviderSettingsResolver<OidcProviderSettings>> resolvers)
    : ProviderSettingsService<OidcApplication, OidcProviderSettings>(
        resolvers,
        e => e.Spec.ProviderSettingsRef,
        e => e.Metadata.NamespaceProperty ?? string.Empty);