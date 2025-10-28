using KubeOps.Abstractions.Finalizer;
using Moira.KubeOps.Controllers;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.PreReconcileSteps;

public class GroupPreConcileSteps(
    EntityFinalizerAttacher<GroupFinalizer, Group> finalizer) : IPreReconcileSteps<Group>
{
    public async Task<bool> ExecuteAsync(Group entity, CancellationToken cancellationToken)
    {
        var result = await finalizer(entity, cancellationToken);

        return !result.Equals(entity);
    }
}