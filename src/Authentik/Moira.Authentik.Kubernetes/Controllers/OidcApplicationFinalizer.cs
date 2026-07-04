using KubeOps.Abstractions.Reconciliation;
using KubeOps.Abstractions.Reconciliation.Finalizer;
using Moira.Authentik.Kubernetes.OidcApplication;
using Moira.Common.Kubernetes.AdapterHandler;

namespace Moira.Authentik.Kubernetes.Controllers;

public class OidcApplicationFinalizer(
    IAdapterHandler<AuthentikOidcApplication> handler) : IEntityFinalizer<AuthentikOidcApplication>
{
    public async Task<ReconciliationResult<AuthentikOidcApplication>> FinalizeAsync(AuthentikOidcApplication entity,
        CancellationToken cancellationToken)
    {
        try
        {
            await handler.HandleDeleteAsync(entity, cancellationToken);
            return ReconciliationResult<AuthentikOidcApplication>.Success(entity);
        }
        catch (Exception ex)
        {
            return ReconciliationResult<AuthentikOidcApplication>.Failure(entity, ex.Message, ex, TimeSpan.FromSeconds(20));
        }
    }
}
