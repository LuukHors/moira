using KubeOps.Abstractions.Finalizer;
using Moira.Common.KubeOps.AdapterHandler;
using Moira.Authentik.KubeOps.Entities;

namespace Moira.Authentik.KubeOps.Controllers;

public class ProviderFinalizer(IAdapterHandler<AuthentikProvider> handler) : IEntityFinalizer<AuthentikProvider>
{
    public Task FinalizeAsync(AuthentikProvider entity, CancellationToken cancellationToken) => handler.HandleDeleteAsync(entity, cancellationToken);
}