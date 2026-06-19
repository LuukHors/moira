using k8s.Models;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using Moira.Common.Exceptions;
using Moira.Common.Models;
using Moira.KubeOps.DependencyProvider;
using Moira.KubeOps.Entities;
using Provider = Moira.KubeOps.Entities.Provider;

namespace Moira.KubeOps.AdapterHandler.DependencyProvider;

public class GroupDependencyProvider(
    IKubernetesClient client,
    IDependencyProvider<Provider, IdPProvider> providerDependencyProvider,
    ILogger<GroupDependencyProvider> logger) : IDependencyProvider<Group, IdPGroup>
{
    public async Task<IdPGroup> ResolveAsync(Group entity, CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting provider {providerNamespace}/{providerName} for group", entity.Spec.ProviderRef.Namespace, entity.Spec.ProviderRef.Name);
        var provider = await client.GetAsync<Provider>(
            entity.Spec.ProviderRef.Name,
            entity.Spec.ProviderRef.Namespace, 
            cancellationToken);
        
        if (provider is null)
        {
            logger.LogDebug("Provider was not found...");
            throw new ProviderNotFoundException(entity.Spec.ProviderRef.Namespace, entity.Spec.ProviderRef.Name);
        }
        
        var idPProvider = await providerDependencyProvider.ResolveAsync(provider, cancellationToken);
        
        return new IdPGroup(
            entity.Namespace(),
            entity.Name(),
            idPProvider,
            new IdPGroupSpec(entity.Spec.DisplayName, entity.Spec.MemberOf, entity.Spec.AutoDelete),
            new IdPGroupStatus(entity.Status.GroupId, entity.Status.DisplayName, entity.Status.MemberOfGroupIds)
        );
    }
}
