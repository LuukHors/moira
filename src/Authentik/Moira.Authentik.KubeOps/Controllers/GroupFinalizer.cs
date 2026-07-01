using KubeOps.Abstractions.Finalizer;
using KubeOps.KubernetesClient;
using Moira.Common.KubeOps.AdapterHandler;
using Moira.Authentik.KubeOps.Entities;
using Moira.Common.KubeOps.Status;

namespace Moira.Authentik.KubeOps.Controllers;

public class GroupFinalizer(
    IAdapterHandler<AuthentikGroup> handler) : IEntityFinalizer<AuthentikGroup>
{
    public async Task FinalizeAsync(AuthentikGroup entity, CancellationToken cancellationToken) => 
        await handler.HandleDeleteAsync(entity, cancellationToken);
}
