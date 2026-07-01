using k8s.Models;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using Moira.Authentik.Application.Models;
using Moira.Authentik.Domain.ProviderSettings;
using Moira.Authentik.KubeOps.Entities;
using Moira.Common.Abstractions.Exceptions;
using Moira.Common.Abstractions.Models;
using Moira.Common.KubeOps.AdapterHandler.DependencyProvider;

namespace Moira.Authentik.KubeOps.AdapterHandler.DependencyProvider;

public class GroupDependencyProvider(
    IKubernetesClient client,
    IDependencyProvider<AuthentikProvider, IdPProvider> providerDependencyProvider,
    ILogger<GroupDependencyProvider> logger) : IDependencyProvider<AuthentikGroup, AuthentikGroupModel>
{
    public async Task<AuthentikGroupModel> ResolveAsync(AuthentikGroup entity, CancellationToken cancellationToken)
    {
        logger.LogDebug("Resolving provider {ProviderNamespace}/{ProviderName} for group", entity.Spec.ProviderRef.Namespace, entity.Spec.ProviderRef.Name);
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

        var settings = new AuthentikGroupProviderSettings
        {
            Attributes = new AuthentikGroupAttributeSettings(new Dictionary<string, string>(entity.Spec.Attributes))
        };

        return new AuthentikGroupModel(
            entity.Namespace(),
            entity.Name(),
            idPProvider,
            new AuthentikGroupSpec(entity.Spec.DisplayName, entity.Spec.MemberOf, entity.Spec.AutoDelete, settings),
            new AuthentikGroupStatus(entity.Status.GroupId, entity.Status.DisplayName, entity.Status.MemberOfGroupIds));
    }
}