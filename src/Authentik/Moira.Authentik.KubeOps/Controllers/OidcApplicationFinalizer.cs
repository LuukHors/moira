using KubeOps.Abstractions.Finalizer;
using KubeOps.KubernetesClient;
using Moira.Common.KubeOps.AdapterHandler;
using Moira.Authentik.KubeOps.Entities;
using Moira.Common.KubeOps.Status;

namespace Moira.Authentik.KubeOps.Controllers;

public class OidcApplicationFinalizer(
    IAdapterHandler<AuthentikOidcApplication> handler) : IEntityFinalizer<AuthentikOidcApplication>
{
    public async Task FinalizeAsync(AuthentikOidcApplication entity, CancellationToken cancellationToken) 
        => await handler.HandleDeleteAsync(entity, cancellationToken);
}
