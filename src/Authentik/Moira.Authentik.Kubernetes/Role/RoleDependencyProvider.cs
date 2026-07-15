using k8s.Models;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using Moira.Authentik.Application.Models;
using Moira.Authentik.Application.Models.Role;
using Moira.Authentik.Kubernetes.Provider;
using Moira.Common.Abstractions.Exceptions;
using Moira.Common.Abstractions.Models;
using Moira.Common.Kubernetes.DependencyProvider;

namespace Moira.Authentik.Kubernetes.Role;

public class RoleDependencyProvider(
    IKubernetesClient client,
    IDependencyProvider<AuthentikProvider, IdPProvider> providerDependencyProvider,
    ILogger<RoleDependencyProvider> logger) : IDependencyProvider<AuthentikRole, AuthentikRoleModel>
{
    public async Task<AuthentikRoleModel> ResolveAsync(AuthentikRole entity, CancellationToken cancellationToken)
    {
        var provider = await client.GetAsync<AuthentikProvider>(
            entity.Spec.ProviderRef.Name,
            entity.Spec.ProviderRef.Namespace,
            cancellationToken);

        if (provider is null)
        {
            logger.LogDebug("Provider {ProviderNamespace}/{ProviderName} was not found", entity.Spec.ProviderRef.Namespace, entity.Spec.ProviderRef.Name);
            throw new ProviderNotFoundException(entity.Spec.ProviderRef.Namespace, entity.Spec.ProviderRef.Name);
        }
        
        var idPProvider = await providerDependencyProvider.ResolveAsync(provider, cancellationToken);
        logger.LogDebug("Resolved provider {ProviderNamespace}/{ProviderName} for group", entity.Spec.ProviderRef.Namespace, entity.Spec.ProviderRef.Name);
        
        return new AuthentikRoleModel(
            entity.Namespace(),
            entity.Name(),
            idPProvider,
            new AuthentikRoleSpec(
                entity.Spec.DisplayName,
                entity.Spec.Permissions
            ),
            new AuthentikRoleStatus(
                entity.Status.RoleId
            )
        );
    }
}