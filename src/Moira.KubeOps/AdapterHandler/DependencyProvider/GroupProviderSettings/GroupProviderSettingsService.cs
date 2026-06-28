using Moira.Common.Models;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.AdapterHandler.DependencyProvider.GroupProviderSettings;

public class GroupProviderSettingsService(IEnumerable<IProviderSettingsResolver<Common.Models.GroupProviderSettings>> resolvers)
    : ProviderSettingsService<Group, Common.Models.GroupProviderSettings>(
        resolvers,
        e => e.Spec.ProviderSettingsRef,
        e => e.Metadata.NamespaceProperty ?? string.Empty);