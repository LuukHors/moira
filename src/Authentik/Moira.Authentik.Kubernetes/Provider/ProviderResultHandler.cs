using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using Moira.Authentik.Kubernetes.Role;
using Moira.Common.Abstractions.Exceptions;
using Moira.Common.Abstractions.Models;
using Moira.Common.Kubernetes.Mappers;
using Moira.Common.Kubernetes.ResultHandler;
using Moira.Common.Kubernetes.Status;

namespace Moira.Authentik.Kubernetes.Provider;

public class ProviderResultHandler(
    IKubernetesClient client,
    ILogger<RoleResultHandler> logger) : IResultHandler<AuthentikProvider, IdPProvider>
{
    public async Task HandleReconcileResultAsync(AuthentikProvider entity, IdPProvider idpEntity, CancellationToken cancellationToken)
    {
        entity.Status.ObservedGeneration = entity.Metadata.Generation;
        entity.UpsertCondition(
            entity.Status.Conditions,
            ConditionTypes.Ready,
            ConditionStatus.True,
            ConditionReasons.ProviderCheckSucceeded,
            "AuthentikProvider configuration is usable.");
        entity.UpsertCondition(
            entity.Status.Conditions,
            ConditionTypes.DependenciesReady,
            ConditionStatus.True,
            ConditionReasons.DependenciesResolved,
            "Referenced credentials were resolved.");

        await client.UpdateStatusAsync(entity, cancellationToken);
        logger.LogDebug("Updated provider status after successful provider check");
    }

    public async Task HandleExceptionAsync(AuthentikProvider entity, MoiraException exception, CancellationToken cancellationToken)
    {
        entity.Status.ObservedGeneration = entity.Metadata.Generation;
        entity.UpsertCondition(
            entity.Status.Conditions,
            ConditionTypes.Ready,
            ConditionStatus.False,
            exception.ToProviderCheckFailureReason(),
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
        logger.LogDebug("Updated provider status after failed operation with reason {FailureReason}", exception.Reason);
    }

    public async Task HandleDeletedAsync(AuthentikProvider entity, IdPProvider idpEntity, CancellationToken cancellationToken)
    {
        entity.Status.ObservedGeneration = entity.Metadata.Generation;
        entity.UpsertCondition(
            entity.Status.Conditions,
            ConditionTypes.Ready,
            ConditionStatus.False,
            ConditionReasons.DeleteSucceeded,
            "AuthentikProvider is being deleted.");

        await client.UpdateStatusAsync(entity, cancellationToken);
        logger.LogDebug("Updated provider status after delete");
    }
}
