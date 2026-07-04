using KubeOps.Abstractions.Controller;
using Moira.Authentik.Kubernetes.OidcApplication;
using Moira.Common.Kubernetes.AdapterHandler;

namespace Moira.Authentik.Kubernetes.Controllers;

internal class OidcApplicationController(IAdapterHandler<AuthentikOidcApplication> handler) : IEntityController<AuthentikOidcApplication>
{
    public Task ReconcileAsync(AuthentikOidcApplication entity, CancellationToken cancellationToken) => handler.HandleReconcileAsync(entity, cancellationToken);
    public Task DeletedAsync(AuthentikOidcApplication entity, CancellationToken cancellationToken) => Task.CompletedTask;
}
