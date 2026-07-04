using KubeOps.Abstractions.Finalizer;
using KubeOps.KubernetesClient;
using Moira.Authentik.Kubernetes.OidcApplication;
using Moira.Common.Kubernetes.AdapterHandler;

namespace Moira.Authentik.Kubernetes.Controllers;

public class OidcApplicationFinalizer(
    IAdapterHandler<AuthentikOidcApplication> handler) : IEntityFinalizer<AuthentikOidcApplication>
{
    public async Task FinalizeAsync(AuthentikOidcApplication entity, CancellationToken cancellationToken) 
        => await handler.HandleDeleteAsync(entity, cancellationToken);
}
