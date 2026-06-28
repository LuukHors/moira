using Moira.Common.Models;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.AdapterHandler.DependencyProvider.GroupProviderSettings;

public class GroupProviderSettingsService(IEnumerable<IProviderSettingsResolver<GroupProviderSettings>> resolvers)
    : ProviderSettingsService<Group, GroupProviderSettings>(
        resolvers,
        e => e.Spec.ProviderSettingsRef,
        e => e.Metadata.NamespaceProperty ?? string.Empty);