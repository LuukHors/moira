using KubeOps.Abstractions.Queue;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using Moira.Common.Exceptions;
using Moira.Common.Models;
using Moira.KubeOps.Entities;
using Moira.KubeOps.Mappers;
using Moira.KubeOps.Status;

namespace Moira.KubeOps.ResultHandler;

public class GroupResultHandler(
    IKubernetesClient client,
    EntityRequeue<Group> entityRequeue,
    ILogger<GroupResultHandler> logger) : IResultHandler<Group, IdPGroup>
{
    public async Task HandleAsync(Group entity, IdPGroup idpEntity, CancellationToken cancellationToken)
    {
        entity.Status.ObservedGeneration = entity.Metadata.Generation;
        entity.Status.DisplayName = idpEntity.Status.DisplayName;
        entity.Status.GroupId = idpEntity.Status.GroupId;
        entity.Status.MemberOfGroupIds = idpEntity.Status.MemberOfGroupIds;

        entity.UpsertCondition(
            entity.Status.Conditions,
            ConditionTypes.Ready,
            ConditionStatus.True,
            ConditionReasons.ReconcileSucceeded,
            "Group has been reconciled with the identity provider.");
        entity.UpsertCondition(
            entity.Status.Conditions,
            ConditionTypes.DependenciesReady,
            ConditionStatus.True,
            ConditionReasons.DependenciesResolved,
            "Referenced provider and credentials were resolved.");
        entity.UpsertCondition(
            entity.Status.Conditions,
            ConditionTypes.Deleting,
            ConditionStatus.False,
            ConditionReasons.ReconcileSucceeded,
            "Group is not being deleted.");

        await client.UpdateStatusAsync(entity, cancellationToken);
        logger.LogDebug("Updated group status after successful reconcile with group id {GroupId}", idpEntity.Status.GroupId);
        
        entityRequeue(entity, TimeSpan.FromSeconds(20));
        logger.LogDebug("Requeued group after successful reconcile with delay {RequeueDelaySeconds}", 20);
    }

    public async Task HandleExceptionAsync(Group entity, MoiraException exception, CancellationToken cancellationToken)
    {
        entity.Status.ObservedGeneration = entity.Metadata.Generation;
        entity.UpsertCondition(
            entity.Status.Conditions,
            ConditionTypes.Ready,
            ConditionStatus.False,
            exception.ToReconcileFailureReason(),
            exception.Message);

        if (exception is DependencyException)
        {
            entity.UpsertCondition(
                entity.Status.Conditions,
                ConditionTypes.DependenciesReady,
                ConditionStatus.False,
                exception.ToDependencyFailureReason(),
                exception.Message);
        }

        if (IsDeleting(entity))
        {
            entity.UpsertCondition(
                entity.Status.Conditions,
                ConditionTypes.Deleting,
                ConditionStatus.False,
                ConditionReasons.DeleteFailed,
                exception.Message);
        }

        await client.UpdateStatusAsync(entity, cancellationToken);
        logger.LogDebug("Updated group status after failed operation with reason {FailureReason}", exception.Reason);
        
        entityRequeue(entity, TimeSpan.FromSeconds(20));
        logger.LogDebug("Requeued group after failed operation with delay {RequeueDelaySeconds}", 20);
    }

    public async Task HandleDeleteAsync(Group entity, IdPGroup idpEntity, CancellationToken cancellationToken)
    {
        entity.Status.ObservedGeneration = entity.Metadata.Generation;

        var reason = idpEntity.Spec.AutoDelete
            ? ConditionReasons.DeleteSucceeded
            : ConditionReasons.DeleteSkipped;
        var message = idpEntity.Spec.AutoDelete
            ? "Group deletion has been handled by the identity provider."
            : "Group deletion was skipped because autoDelete is disabled.";

        entity.UpsertCondition(
            entity.Status.Conditions,
            ConditionTypes.Deleting,
            ConditionStatus.False,
            reason,
            message);

        await client.UpdateStatusAsync(entity, cancellationToken);
        logger.LogDebug("Updated group status after delete with reason {DeleteReason}", reason);
    }

    private static bool IsDeleting(Group entity)
    {
        return entity.Status.Conditions.Any(condition =>
            condition.Type == ConditionTypes.Deleting
            && condition.Status == ConditionStatus.True);
    }
}
