using KubeOps.Abstractions.Finalizer;
using Moira.KubeOps.AdapterHandler;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.Controllers;

public class GroupFinalizer(IAdapterHandler<Group> handler) : IEntityFinalizer<Group>
{
    public Task FinalizeAsync(Group entity, CancellationToken cancellationToken) => handler.HandleDeleteAsync(entity, cancellationToken);
}