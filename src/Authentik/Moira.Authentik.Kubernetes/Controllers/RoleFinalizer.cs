using KubeOps.Abstractions.Reconciliation;
using KubeOps.Abstractions.Reconciliation.Finalizer;
using Moira.Authentik.Kubernetes.Role;
using Moira.Common.Kubernetes.AdapterHandler;

namespace Moira.Authentik.Kubernetes.Controllers;

public class RoleFinalizer(IAdapterHandler<AuthentikRole> handler) : IEntityFinalizer<AuthentikRole>
{
    public async Task<ReconciliationResult<AuthentikRole>> FinalizeAsync(AuthentikRole entity, CancellationToken cancellationToken)
    {
        try
        {
            await handler.HandleDeleteAsync(entity, cancellationToken);
            return ReconciliationResult<AuthentikRole>.Success(entity);
        }
        catch (Exception ex)
        {
            return ReconciliationResult<AuthentikRole>.Failure(entity, ex.Message, ex, TimeSpan.FromSeconds(20));
        }
    }
}
