using KubeOps.Abstractions.Reconciliation;
using KubeOps.Abstractions.Reconciliation.Controller;
using Moira.Authentik.Kubernetes.OidcApplication;
using Moira.Common.Kubernetes.AdapterHandler;

namespace Moira.Authentik.Kubernetes.Controllers;

internal class OidcApplicationController(IAdapterHandler<AuthentikOidcApplication> handler) : IEntityController<AuthentikOidcApplication>
{
    public async Task<ReconciliationResult<AuthentikOidcApplication>> ReconcileAsync(AuthentikOidcApplication entity, CancellationToken cancellationToken)
    {
        try
        {
            await handler.HandleReconcileAsync(entity, cancellationToken);
            return ReconciliationResult<AuthentikOidcApplication>.Success(entity, TimeSpan.FromSeconds(20));
        }
        catch (Exception ex)
        {
            return ReconciliationResult<AuthentikOidcApplication>.Failure(entity, ex.Message, ex, TimeSpan.FromSeconds(20));
        }
    }

    public Task<ReconciliationResult<AuthentikOidcApplication>> DeletedAsync(AuthentikOidcApplication entity, CancellationToken cancellationToken)
    {
        return Task.FromResult(ReconciliationResult<AuthentikOidcApplication>.Success(entity));
    }
}
