using KubeOps.Abstractions.Controller;
using Moira.Authentik.Kubernetes.Group;
using Moira.Common.Kubernetes.AdapterHandler;

namespace Moira.Authentik.Kubernetes.Controllers;

internal class GroupController(IAdapterHandler<AuthentikGroup> handler) : IEntityController<AuthentikGroup>
{
    public Task ReconcileAsync(AuthentikGroup entity, CancellationToken cancellationToken) => handler.HandleReconcileAsync(entity, cancellationToken);
    public Task DeletedAsync(AuthentikGroup entity, CancellationToken cancellationToken) => Task.CompletedTask;
}