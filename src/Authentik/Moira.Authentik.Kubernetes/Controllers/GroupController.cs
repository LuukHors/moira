using KubeOps.Abstractions.Reconciliation;
using KubeOps.Abstractions.Reconciliation.Controller;
using Moira.Authentik.Kubernetes.Group;
using Moira.Common.Kubernetes.AdapterHandler;

namespace Moira.Authentik.Kubernetes.Controllers;

internal class GroupController(IAdapterHandler<AuthentikGroup> handler) : IEntityController<AuthentikGroup>
{
    public async Task<ReconciliationResult<AuthentikGroup>> ReconcileAsync(AuthentikGroup entity, CancellationToken cancellationToken)
    {
        try
        {
            await handler.HandleReconcileAsync(entity, cancellationToken);
            return ReconciliationResult<AuthentikGroup>.Success(entity, TimeSpan.FromSeconds(20));
        }
        catch (Exception ex)
        {
            return ReconciliationResult<AuthentikGroup>.Failure(entity, ex.Message, ex, TimeSpan.FromSeconds(20));
        }
    }

    public Task<ReconciliationResult<AuthentikGroup>> DeletedAsync(AuthentikGroup entity,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(ReconciliationResult<AuthentikGroup>.Success(entity));
    }
}