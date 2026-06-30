using k8s.Models;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using Moira.Common.Abstractions;
using Moira.Common.Abstractions.Exceptions;
using Moira.Common.Abstractions.Models;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.AdapterHandler.DependencyProvider;

public class GroupDependencyProvider(
    IKubernetesClient client,
    IDependencyProvider<Provider, IdPProvider> providerDependencyProvider,
    IProviderSettingsService providerSettingsService,
    ILogger<GroupDependencyProvider> logger) : IDependencyProvider<Group, IdPGroup>
{
    public async Task<IdPGroup> ResolveAsync(Group entity, CancellationToken cancellationToken)
    {
        logger.LogDebug("Resolving provider {ProviderNamespace}/{ProviderName} for group", entity.Spec.ProviderRef.Namespace, entity.Spec.ProviderRef.Name);
        var provider = await client.GetAsync<Provider>(
            entity.Spec.ProviderRef.Name,
            entity.Spec.ProviderRef.Namespace,
            cancellationToken);

        if (provider is null)
        {
            logger.LogDebug("Provider {ProviderNamespace}/{ProviderName} was not found", entity.Spec.ProviderRef.Namespace, entity.Spec.ProviderRef.Name);
            throw new ProviderNotFoundException(entity.Spec.ProviderRef.Namespace, entity.Spec.ProviderRef.Name);
        }

        var idPProvider = await providerDependencyProvider.ResolveAsync(provider, cancellationToken);
        var providerSettings = await providerSettingsService.ResolveAsync(entity.Spec.ProviderSettingsRef, cancellationToken);
        logger.LogDebug("Resolved provider {ProviderNamespace}/{ProviderName} for group", entity.Spec.ProviderRef.Namespace, entity.Spec.ProviderRef.Name);

        return new IdPGroup(
            entity.Namespace(),
            entity.Name(),
            idPProvider,
            new IdPGroupSpec(entity.Spec.DisplayName, entity.Spec.MemberOf, entity.Spec.AutoDelete, providerSettings),
            new IdPGroupStatus(entity.Status.GroupId, entity.Status.DisplayName, entity.Status.MemberOfGroupIds));
    }
}
