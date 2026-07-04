using KubeOps.Abstractions.Finalizer;
using KubeOps.KubernetesClient;
using Moira.Authentik.Kubernetes.Group;
using Moira.Common.Kubernetes.AdapterHandler;

namespace Moira.Authentik.Kubernetes.Controllers;

public class GroupFinalizer(
    IAdapterHandler<AuthentikGroup> handler) : IEntityFinalizer<AuthentikGroup>
{
    public async Task FinalizeAsync(AuthentikGroup entity, CancellationToken cancellationToken) => 
        await handler.HandleDeleteAsync(entity, cancellationToken);
}
