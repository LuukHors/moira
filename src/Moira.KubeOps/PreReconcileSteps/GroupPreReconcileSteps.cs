using KubeOps.Abstractions.Finalizer;
using Microsoft.Extensions.Logging;
using Moira.KubeOps.Controllers;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.PreReconcileSteps;

public class GroupPreReconcileSteps(
    EntityFinalizerAttacher<GroupFinalizer, Group> finalizer,
    ILogger<GroupPreReconcileSteps> logger) : IPreReconcileSteps<Group>
{
    public async Task<bool> ExecuteAsync(Group entity, CancellationToken cancellationToken)
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
