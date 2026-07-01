using KubeOps.Abstractions.Controller;
using Moira.Common.KubeOps.AdapterHandler;
using Moira.Authentik.KubeOps.Entities;

namespace Moira.Authentik.KubeOps.Controllers;

internal class GroupController(IAdapterHandler<AuthentikGroup> handler) : IEntityController<AuthentikGroup>
{
    public Task ReconcileAsync(AuthentikGroup entity, CancellationToken cancellationToken) => handler.HandleReconcileAsync(entity, cancellationToken);
    public Task DeletedAsync(AuthentikGroup entity, CancellationToken cancellationToken) => Task.CompletedTask;
}