using KubeOps.Abstractions.Controller;
using Moira.Authentik.Kubernetes.Provider;
using Moira.Common.Kubernetes.AdapterHandler;

namespace Moira.Authentik.Kubernetes.Controllers;

internal class ProviderController(IAdapterHandler<AuthentikProvider> handler) : IEntityController<AuthentikProvider>
{
    public Task ReconcileAsync(AuthentikProvider entity, CancellationToken cancellationToken) => handler.HandleReconcileAsync(entity, cancellationToken);
    public Task DeletedAsync(AuthentikProvider entity, CancellationToken cancellationToken) => Task.CompletedTask;
}