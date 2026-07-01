using KubeOps.Abstractions.Controller;
using Moira.Common.KubeOps.AdapterHandler;
using Moira.Authentik.KubeOps.Entities;

namespace Moira.Authentik.KubeOps.Controllers;

internal class OidcApplicationController(IAdapterHandler<AuthentikOidcApplication> handler) : IEntityController<AuthentikOidcApplication>
{
    public Task ReconcileAsync(AuthentikOidcApplication entity, CancellationToken cancellationToken) => handler.HandleReconcileAsync(entity, cancellationToken);
    public Task DeletedAsync(AuthentikOidcApplication entity, CancellationToken cancellationToken) => Task.CompletedTask;
}
