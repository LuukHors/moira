using KubeOps.Abstractions.Controller;
using Moira.KubeOps.AdapterHandler;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.Controllers;

internal class OidcApplicationController(IAdapterHandler<OidcApplication> handler) : IEntityController<OidcApplication>
{
    public Task ReconcileAsync(OidcApplication entity, CancellationToken cancellationToken) => handler.HandleReconcileAsync(entity, cancellationToken);
    public Task DeletedAsync(OidcApplication entity, CancellationToken cancellationToken) => Task.CompletedTask;
}
