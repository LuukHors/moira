using KubeOps.Abstractions.Finalizer;
using Moira.Authentik.Kubernetes.Provider;
using Moira.Common.Kubernetes.AdapterHandler;

namespace Moira.Authentik.Kubernetes.Controllers;

public class ProviderFinalizer(IAdapterHandler<AuthentikProvider> handler) : IEntityFinalizer<AuthentikProvider>
{
    public Task FinalizeAsync(AuthentikProvider entity, CancellationToken cancellationToken) => handler.HandleDeleteAsync(entity, cancellationToken);
}