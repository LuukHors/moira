using KubeOps.Abstractions.Reconciliation.Finalizer;
using KubeOps.KubernetesClient;
using Moira.Authentik.Kubernetes.Controllers;
using Moira.Common.Kubernetes.PreReconcileSteps;

namespace Moira.Authentik.Kubernetes.Role;

public class RolePreReconcileSteps(EntityFinalizerAttacher<RoleFinalizer, AuthentikRole> finalizer) : IPreReconcileSteps<AuthentikRole>
{
    public async Task<bool> ExecuteAsync(AuthentikRole entity, CancellationToken cancellationToken)
    {
        var result = await finalizer(entity, cancellationToken);
        return !result.Equals(entity);
    }
}
