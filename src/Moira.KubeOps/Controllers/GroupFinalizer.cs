using KubeOps.Abstractions.Finalizer;
using KubeOps.KubernetesClient;
using Moira.KubeOps.AdapterHandler;
using Moira.KubeOps.Entities;
using Moira.KubeOps.Status;

namespace Moira.KubeOps.Controllers;

public class GroupFinalizer(
    IAdapterHandler<Group> handler) : IEntityFinalizer<Group>
{
    public async Task FinalizeAsync(Group entity, CancellationToken cancellationToken) => 
        await handler.HandleDeleteAsync(entity, cancellationToken);
}
