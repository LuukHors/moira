using KubeOps.Abstractions.Reconciliation;
using KubeOps.Abstractions.Reconciliation.Finalizer;
using Moira.Authentik.Kubernetes.Provider;
using Moira.Common.Kubernetes.AdapterHandler;

namespace Moira.Authentik.Kubernetes.Controllers;

public class ProviderFinalizer(IAdapterHandler<AuthentikProvider> handler) : IEntityFinalizer<AuthentikProvider>
{
    public async Task<ReconciliationResult<AuthentikProvider>> FinalizeAsync(AuthentikProvider entity, CancellationToken cancellationToken)
    {
        try
        {
            await handler.HandleDeleteAsync(entity, cancellationToken);
            return ReconciliationResult<AuthentikProvider>.Success(entity);
        }
        catch (Exception ex)
        {
            return ReconciliationResult<AuthentikProvider>.Failure(entity, ex.Message, ex, TimeSpan.FromSeconds(20));
        }
    }
}