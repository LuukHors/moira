using KubeOps.Abstractions.Controller;
using Microsoft.Extensions.Logging;
using Moira.KubeOps.AdapterHandler;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.Controllers;

public class GroupController(IAdapterHandler<Group> handler) : IEntityController<Group>
{
    public async Task ReconcileAsync(Group entity, CancellationToken cancellationToken) => await handler.HandleReconcileAsync(entity, cancellationToken);
    public Task DeletedAsync(Group entity, CancellationToken cancellationToken) => Task.CompletedTask;
}