using KubeOps.Abstractions.Reconciliation;
using KubeOps.Abstractions.Reconciliation.Finalizer;
using Moira.Authentik.Kubernetes.Group;
using Moira.Common.Kubernetes.AdapterHandler;

namespace Moira.Authentik.Kubernetes.Controllers;

public class GroupFinalizer(IAdapterHandler<AuthentikGroup> handler) : IEntityFinalizer<AuthentikGroup>
{
    public async Task<ReconciliationResult<AuthentikGroup>> FinalizeAsync(AuthentikGroup entity, CancellationToken cancellationToken)
    {
        try
        {
            await handler.HandleDeleteAsync(entity, cancellationToken);
            return ReconciliationResult<AuthentikGroup>.Success(entity);
        }
        catch (Exception ex)
        {
            return ReconciliationResult<AuthentikGroup>.Failure(entity, ex.Message, ex, TimeSpan.FromSeconds(20));
        }
    }
}
