using Moira.Common.KubeOps.PreReconcileSteps;
using KubeOps.Abstractions.Finalizer;
using Microsoft.Extensions.Logging;
using Moira.Authentik.KubeOps.Controllers;
using Moira.Authentik.KubeOps.Entities;

namespace Moira.Authentik.KubeOps.PreReconcileSteps;

public class GroupPreReconcileSteps(
    EntityFinalizerAttacher<GroupFinalizer, AuthentikGroup> finalizer,
    ILogger<GroupPreReconcileSteps> logger) : IPreReconcileSteps<AuthentikGroup>
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
