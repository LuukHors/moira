using k8s.Models;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using Moira.Common.Models;
using Moira.Common.RequestContext;
using Moira.KubeOps.Entities;
using Provider = Moira.KubeOps.Entities.Provider;

namespace Moira.KubeOps.DependencyProvider;

public class GroupDependencyProvider(
    IKubernetesClient client,
    IDependencyProvider<Provider, IdPProvider> providerDependencyProvider,
    IRequestContextProvider requestContext,
    ILogger<GroupDependencyProvider> logger
    ) : IDependencyProvider<Group, IdPGroup>
{
    public async Task<IdPGroup> ResolveAsync(Group entity, CancellationToken cancellationToken)
    {
        logger.LogDebug("[{commandId}][IdPGroup][{entityName}] Getting provider {providerNamespace}/{providerName} for group", requestContext.RequestId, entity.Name(), entity.Spec.ProviderRef.Namespace, entity.Spec.ProviderRef.Name);
        var provider = await client.GetAsync<Provider>(
            entity.Spec.ProviderRef.Name,
            entity.Spec.ProviderRef.Namespace, 
            cancellationToken);
        
        if (provider is null)
        {
            logger.LogDebug("[{commandId}][IdPGroup][{entityName}] Provider was not found...", requestContext.RequestId, entity.Name());
            throw new InvalidOperationException($"Unable to get provider with name \"{entity.Spec.ProviderRef.Name}\" in namespace \"{entity.Namespace()}\"");
        }
        
        var idPProvider = await providerDependencyProvider.ResolveAsync(provider, cancellationToken);
        
        return new IdPGroup(
            entity.Namespace(),
            entity.Name(),
            idPProvider,
            new IdPGroupSpec(entity.Spec.DisplayName, entity.Spec.MemberOf),
            new IdPGroupStatus(entity.Status.GroupId, entity.Status.DisplayName, entity.Status.MemberOfGroupIds)
        );
    }
}