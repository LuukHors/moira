using KubeOps.Abstractions.Finalizer;
using Moira.KubeOps.AdapterHandler;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.Controllers;

public class ProviderFinalizer(IAdapterHandler<Provider> handler) : IEntityFinalizer<Provider>
{
    public Task FinalizeAsync(Provider entity, CancellationToken cancellationToken) => handler.HandleDeleteAsync(entity, cancellationToken);
}