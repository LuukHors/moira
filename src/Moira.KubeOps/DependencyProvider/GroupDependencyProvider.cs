using k8s.Models;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using Moira.Common.Models;
using Moira.KubeOps.Entities;
using Provider = Moira.KubeOps.Entities.Provider;

namespace Moira.KubeOps.DependencyProvider;

public class GroupDependencyProvider(
    IKubernetesClient client,
    IDependencyProvider<Provider, IdPProvider> providerDependencyProvider,
    ILogger<GroupDependencyProvider> logger
    ) : IDependencyProvider<Group, IdPGroup>
{
    public async Task<IdPGroup> ResolveAsync(Group entity, CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting provider for group {groupName} from kubernetes api with name {providerName}", entity.Name(), entity.Spec.ProviderRef.Name);
        var provider = await client.GetAsync<Provider>(
            entity.Spec.ProviderRef.Name,
            entity.Namespace(), 
            cancellationToken);
        
        if (provider is null)
        {
            logger.LogDebug("Could not get provider for group {groupName} from kubernetes api with name {providerName}", entity.Name(), entity.Spec.ProviderRef.Name);
            throw new DependencyException($"Unable to get provider with name \"{entity.Spec.ProviderRef.Name}\" in namespace \"{entity.Namespace()}\"");
        }
        
        var idPProvider = await providerDependencyProvider.ResolveAsync(provider, cancellationToken);
        
        
        logger.LogDebug("Got provider for group {groupName} from kubernetes api with name {providerName}", entity.Name(), entity.Spec.ProviderRef.Name);
        return new IdPGroup(
            entity.Namespace(),
            entity.Name(),
            idPProvider,
            new IdPGroupSpec(entity.Spec.DisplayName, entity.Spec.MemberOf),
            new IdPGroupStatus()
        );
    }
}