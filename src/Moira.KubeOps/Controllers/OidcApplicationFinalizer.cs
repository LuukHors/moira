using KubeOps.Abstractions.Finalizer;
using Moira.KubeOps.AdapterHandler;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.Controllers;

public class OidcApplicationFinalizer(IAdapterHandler<OidcApplication> handler) : IEntityFinalizer<OidcApplication>
{
    public async Task FinalizeAsync(OidcApplication entity, CancellationToken cancellationToken) => await handler.HandleDeleteAsync(entity, cancellationToken);
}