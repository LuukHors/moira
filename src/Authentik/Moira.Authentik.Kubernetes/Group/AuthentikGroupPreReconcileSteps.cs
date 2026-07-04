using KubeOps.Abstractions.Reconciliation.Finalizer;
using Microsoft.Extensions.Logging;
using Moira.Authentik.Kubernetes.Controllers;
using Moira.Common.Kubernetes.PreReconcileSteps;

namespace Moira.Authentik.Kubernetes.Group;

public class AuthentikGroupPreReconcileSteps(
    EntityFinalizerAttacher<GroupFinalizer, AuthentikGroup> finalizer,
    ILogger<AuthentikGroupPreReconcileSteps> logger) : IPreReconcileSteps<AuthentikGroup>
{
    public async Task<bool> ExecuteAsync(AuthentikGroup entity, CancellationToken cancellationToken)
    {
        var result = await finalizer(entity, cancellationToken);
        var finalizerAttached = !result.Equals(entity);
        if (finalizerAttached)
        {
            logger.LogInformation("Attached group finalizer; reconciliation will continue after Kubernetes stores the update");
        }

        return finalizerAttached;
    }
}
