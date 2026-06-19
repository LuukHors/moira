using KubeOps.Abstractions.Queue;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using Moira.Common.Exceptions;
using Moira.Common.Models;
using Moira.KubeOps.Entities;
using Moira.KubeOps.Mappers;
using Moira.KubeOps.Status;

namespace Moira.KubeOps.ResultHandler;

public class ProviderResultHandler(
    IKubernetesClient client,
    EntityRequeue<Provider> entityRequeue,
    ILogger<ProviderResultHandler> logger) : IResultHandler<Provider, IdPProvider>
{
    public async Task HandleAsync(Provider entity, IdPProvider idpEntity, CancellationToken cancellationToken)
    {
        entity.Status.ObservedGeneration = entity.Metadata.Generation;
        entity.UpsertCondition(
            entity.Status.Conditions,
            ConditionTypes.Ready,
            ConditionStatus.True,
            ConditionReasons.ProviderCheckSucceeded,
            "Provider configuration is usable.");
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

    public async Task HandleExceptionAsync(Provider entity, MoiraException exception, CancellationToken cancellationToken)
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

    public async Task HandleDeleteAsync(Provider entity, IdPProvider idpEntity, CancellationToken cancellationToken)
    {
        entity.Status.ObservedGeneration = entity.Metadata.Generation;
        entity.UpsertCondition(
            entity.Status.Conditions,
            ConditionTypes.Ready,
            ConditionStatus.False,
            ConditionReasons.DeleteSucceeded,
            "Provider is being deleted.");

        await client.UpdateStatusAsync(entity, cancellationToken);
        logger.LogDebug("Updated provider status after delete");
    }
}
