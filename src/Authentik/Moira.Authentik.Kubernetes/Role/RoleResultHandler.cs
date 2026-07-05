using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using Moira.Authentik.Application.Models.Role;
using Moira.Authentik.Kubernetes.Provider;
using Moira.Common.Abstractions.Exceptions;
using Moira.Common.Abstractions.Models;
using Moira.Common.Kubernetes.Mappers;
using Moira.Common.Kubernetes.ResultHandler;
using Moira.Common.Kubernetes.Status;

namespace Moira.Authentik.Kubernetes.Role;

public class RoleResultHandler(
    IKubernetesClient client,
    ILogger<RoleResultHandler> logger) : IResultHandler<AuthentikRole, AuthentikRoleModel>
{
    public async Task HandleReconcileResultAsync(AuthentikRole entity, AuthentikRoleModel idpEntity, CancellationToken cancellationToken)
    {
        entity.Status.ObservedGeneration = entity.Metadata.Generation;
        entity.Status.RoleId = idpEntity.Status.RoleId;
        entity.UpsertCondition(
            entity.Status.Conditions,
            ConditionTypes.Ready,
            ConditionStatus.True,
            ConditionReasons.ReconcileSucceeded,
            "AuthentikRole has been reconciled with the identity provider.");
        entity.UpsertCondition(
            entity.Status.Conditions,
            ConditionTypes.DependenciesReady,
            ConditionStatus.True,
            ConditionReasons.DependenciesResolved,
            "Referenced credentials were resolved.");

        await client.UpdateStatusAsync(entity, cancellationToken);
        logger.LogDebug("Updated role status after successful provider check");
    }

    public async Task HandleExceptionAsync(AuthentikRole entity, MoiraException exception, CancellationToken cancellationToken)
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
        else
        {
            entity.UpsertCondition(
                entity.Status.Conditions,
                ConditionTypes.DependenciesReady,
                ConditionStatus.True,
                ConditionReasons.DependenciesResolved,
                "Referenced credentials were resolved.");
        }

        await client.UpdateStatusAsync(entity, cancellationToken);
        logger.LogDebug("Updated role status after failed operation with reason {FailureReason}", exception.Reason);
    }

    public Task HandleDeletedAsync(AuthentikRole entity, AuthentikRoleModel idpEntity, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
