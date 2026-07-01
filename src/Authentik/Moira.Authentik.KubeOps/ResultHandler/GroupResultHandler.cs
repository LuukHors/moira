using Moira.Authentik.Application.Models;
using Moira.Common.KubeOps.ResultHandler;
using k8s.Models;
using KubeOps.Abstractions.Queue;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using Moira.Common.Abstractions.Exceptions;
using Moira.Common.Abstractions.Models;
using Moira.Authentik.KubeOps.Entities;
using Moira.Common.KubeOps.Mappers;
using Moira.Common.KubeOps.Status;

namespace Moira.Authentik.KubeOps.ResultHandler;

public class GroupResultHandler(
    IKubernetesClient client,
    EntityRequeue<AuthentikGroup> entityRequeue,
    ILogger<GroupResultHandler> logger) : IResultHandler<AuthentikGroup, AuthentikGroupModel>
{
    public async Task HandleAsync(AuthentikGroup entity, AuthentikGroupModel idpEntity, CancellationToken cancellationToken)
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
            "AuthentikGroup has been reconciled with the identity provider.");
        entity.UpsertCondition(
            entity.Status.Conditions,
            ConditionTypes.DependenciesReady,
            ConditionStatus.True,
            ConditionReasons.DependenciesResolved,
            "Referenced provider and credentials were resolved.");

        await client.UpdateStatusAsync(entity, cancellationToken);
        logger.LogDebug("Updated group status after successful reconcile with group id {GroupId}", idpEntity.Status.GroupId);
        
        entityRequeue(entity, TimeSpan.FromSeconds(20));
        logger.LogDebug("Requeued group after successful reconcile with delay {RequeueDelaySeconds}", 20);
    }

    public async Task HandleExceptionAsync(AuthentikGroup entity, MoiraException exception, CancellationToken cancellationToken)
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

        await client.UpdateStatusAsync(entity, cancellationToken);
        logger.LogDebug("Updated group status after failed operation with reason {FailureReason}", exception.Reason);
        
        entityRequeue(entity, TimeSpan.FromSeconds(20));
        logger.LogDebug("Requeued group after failed operation with delay {RequeueDelaySeconds}", 20);
    }

    public Task HandleDeleteAsync(AuthentikGroup entity, AuthentikGroupModel idpEntity, CancellationToken cancellationToken) => Task.CompletedTask;

    private static bool IsDeleting(AuthentikGroup entity)
    {
        return entity.Status.Conditions.Any(condition =>
            condition.Type == ConditionTypes.Deleting
            && condition.Status == ConditionStatus.True);
    }
}
