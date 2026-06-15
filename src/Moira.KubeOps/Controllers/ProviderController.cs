using KubeOps.Abstractions.Controller;
using Moira.KubeOps.AdapterHandler;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.Controllers;

internal class ProviderController(IAdapterHandler<Provider> handler) : IEntityController<Provider>
{
    public Task ReconcileAsync(Provider entity, CancellationToken cancellationToken) => handler.HandleReconcileAsync(entity, cancellationToken);
    public Task DeletedAsync(Provider entity, CancellationToken cancellationToken) => Task.CompletedTask;
}