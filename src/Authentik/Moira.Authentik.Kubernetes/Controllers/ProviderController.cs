using KubeOps.Abstractions.Reconciliation;
using KubeOps.Abstractions.Reconciliation.Controller;
using Moira.Authentik.Kubernetes.Provider;
using Moira.Common.Kubernetes.AdapterHandler;

namespace Moira.Authentik.Kubernetes.Controllers;

internal class ProviderController(IAdapterHandler<AuthentikProvider> handler) : IEntityController<AuthentikProvider>
{
    public async Task<ReconciliationResult<AuthentikProvider>> ReconcileAsync(AuthentikProvider entity, CancellationToken cancellationToken)
    {
        try
        {
            await handler.HandleReconcileAsync(entity, cancellationToken);
            return ReconciliationResult<AuthentikProvider>.Success(entity, TimeSpan.FromSeconds(20));
        }
        catch (Exception ex)
        {
            return ReconciliationResult<AuthentikProvider>.Failure(entity, ex.Message, ex, TimeSpan.FromSeconds(20));
        }
    }

    public Task<ReconciliationResult<AuthentikProvider>> DeletedAsync(AuthentikProvider entity, CancellationToken cancellationToken)
    {
        return Task.FromResult(ReconciliationResult<AuthentikProvider>.Success(entity));
    }
}