using Moira.Common.Models;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.AdapterHandler.DependencyProvider.OidcProviderSettings;

public class OidcProviderSettingsService(IEnumerable<IProviderSettingsResolver<Common.Models.OidcProviderSettings>> resolvers)
    : ProviderSettingsService<OidcApplication, Common.Models.OidcProviderSettings>(
        resolvers,
        e => e.Spec.ProviderSettingsRef,
        e => e.Metadata.NamespaceProperty ?? string.Empty);