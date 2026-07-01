using Moira.Common.KubeOps.ResultHandler;
using KubeOps.Abstractions.Queue;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using Moira.Common.Abstractions.Exceptions;
using Moira.Common.Abstractions.Models;
using Moira.Authentik.KubeOps.Entities;
using Moira.Common.KubeOps.Mappers;
using Moira.Common.KubeOps.Status;

namespace Moira.Authentik.KubeOps.ResultHandler;

public class ProviderResultHandler(
    IKubernetesClient client,
    EntityRequeue<AuthentikProvider> entityRequeue,
    ILogger<ProviderResultHandler> logger) : IResultHandler<AuthentikProvider, IdPProvider>
{
    public async Task HandleAsync(AuthentikProvider entity, IdPProvider idpEntity, CancellationToken cancellationToken)
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
        
        entityRequeue(entity, TimeSpan.FromSeconds(20));
        logger.LogDebug("Requeued provider after successful provider check with delay {RequeueDelaySeconds}", 20);
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
        
        entityRequeue(entity, TimeSpan.FromSeconds(20));
        logger.LogDebug("Requeued provider after failed operation with delay {RequeueDelaySeconds}", 20);
    }

    public async Task HandleDeleteAsync(AuthentikProvider entity, IdPProvider idpEntity, CancellationToken cancellationToken)
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
