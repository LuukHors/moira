using KubeOps.Abstractions.Controller;
using Moira.KubeOps.AdapterHandler;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.Controllers;

internal class OidcApplicationController(IAdapterHandler<OidcApplication> handler) : IEntityController<OidcApplication>
{
    // public async Task ReconcileAsync(OidcApplication entity, CancellationToken cancellationToken) => await handler.HandleReconcileAsync(entity, cancellationToken);
    public Task ReconcileAsync(OidcApplication entity, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task DeletedAsync(OidcApplication entity, CancellationToken cancellationToken) => Task.CompletedTask;
}