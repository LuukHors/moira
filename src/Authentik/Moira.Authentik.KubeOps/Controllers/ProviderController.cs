using KubeOps.Abstractions.Controller;
using Moira.Common.KubeOps.AdapterHandler;
using Moira.Authentik.KubeOps.Entities;

namespace Moira.Authentik.KubeOps.Controllers;

internal class ProviderController(IAdapterHandler<AuthentikProvider> handler) : IEntityController<AuthentikProvider>
{
    public Task ReconcileAsync(AuthentikProvider entity, CancellationToken cancellationToken) => handler.HandleReconcileAsync(entity, cancellationToken);
    public Task DeletedAsync(AuthentikProvider entity, CancellationToken cancellationToken) => Task.CompletedTask;
}