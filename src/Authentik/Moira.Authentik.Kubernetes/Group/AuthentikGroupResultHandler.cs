using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using Moira.Authentik.Application.Models;
using Moira.Authentik.Application.Models.Group;
using Moira.Common.Abstractions.Exceptions;
using Moira.Common.Kubernetes.Mappers;
using Moira.Common.Kubernetes.ResultHandler;
using Moira.Common.Kubernetes.Status;

namespace Moira.Authentik.Kubernetes.Group;

public class AuthentikGroupResultHandler(
    IKubernetesClient client,
    ILogger<AuthentikGroupResultHandler> logger) : IResultHandler<AuthentikGroup, AuthentikGroupModel>
{
    public async Task HandleReconcileResultAsync(AuthentikGroup entity, AuthentikGroupModel idpEntity, CancellationToken cancellationToken)
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
    }

    public Task HandleDeletedAsync(AuthentikGroup entity, AuthentikGroupModel idpEntity, CancellationToken cancellationToken) => Task.CompletedTask;
}
