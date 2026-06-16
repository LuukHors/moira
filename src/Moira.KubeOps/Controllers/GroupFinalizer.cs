using KubeOps.Abstractions.Finalizer;
using KubeOps.KubernetesClient;
using Moira.KubeOps.AdapterHandler;
using Moira.KubeOps.Entities;
using Moira.KubeOps.Status;

namespace Moira.KubeOps.Controllers;

public class GroupFinalizer(
    IAdapterHandler<Group> handler,
    IKubernetesClient client) : IEntityFinalizer<Group>
{
    public async Task FinalizeAsync(Group entity, CancellationToken cancellationToken)
    {
        entity.Status.ObservedGeneration = entity.Metadata.Generation;
        entity.UpsertCondition(
            entity.Status.Conditions,
            ConditionTypes.Deleting,
            ConditionStatus.True,
            ConditionReasons.DeleteStarted,
            "Group deletion is being handled by the identity provider.");
        await client.UpdateStatusAsync(entity, cancellationToken);

        await handler.HandleDeleteAsync(entity, cancellationToken);
    }
}
