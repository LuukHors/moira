using KubeOps.Abstractions.Controller;
using Moira.KubeOps.AdapterHandler;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.Controllers;

internal class GroupController(IAdapterHandler<Group> handler) : IEntityController<Group>
{
    public Task ReconcileAsync(Group entity, CancellationToken cancellationToken) => handler.HandleReconcileAsync(entity, cancellationToken);
    public Task DeletedAsync(Group entity, CancellationToken cancellationToken) => Task.CompletedTask;
}