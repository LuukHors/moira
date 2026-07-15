using KubeOps.Abstractions.Reconciliation;
using KubeOps.Abstractions.Reconciliation.Controller;
using Moira.Authentik.Kubernetes.Group;
using Moira.Authentik.Kubernetes.Role;
using Moira.Common.Kubernetes.AdapterHandler;

namespace Moira.Authentik.Kubernetes.Controllers;

internal class RoleController(IAdapterHandler<AuthentikRole> handler) : IEntityController<AuthentikRole>
{
    public async Task<ReconciliationResult<AuthentikRole>> ReconcileAsync(AuthentikRole entity, CancellationToken cancellationToken)
    {
        try
        {
            await handler.HandleReconcileAsync(entity, cancellationToken);
            return ReconciliationResult<AuthentikRole>.Success(entity, TimeSpan.FromSeconds(20));
        }
        catch (Exception ex)
        {
            return ReconciliationResult<AuthentikRole>.Failure(entity, ex.Message, ex, TimeSpan.FromSeconds(20));
        }
    }

    public Task<ReconciliationResult<AuthentikRole>> DeletedAsync(AuthentikRole entity,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(ReconciliationResult<AuthentikRole>.Success(entity));
    }
}