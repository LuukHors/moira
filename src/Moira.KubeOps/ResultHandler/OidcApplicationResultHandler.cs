using KubeOps.Abstractions.Queue;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using Moira.Common.Exceptions;
using Moira.Common.Models;
using Moira.KubeOps.Entities;
using Moira.KubeOps.Mappers;
using Moira.KubeOps.Secrets;
using Moira.KubeOps.Status;

namespace Moira.KubeOps.ResultHandler;

public class OidcApplicationResultHandler(
    IKubernetesClient client,
    IOidcApplicationSecretService secretService,
    EntityRequeue<OidcApplication> entityRequeue,
    ILogger<OidcApplicationResultHandler> logger) : IResultHandler<OidcApplication, IdPOidcApplication>
{
    public async Task HandleAsync(OidcApplication entity, IdPOidcApplication idpEntity, CancellationToken cancellationToken)
    {
        var secretTargetStatuses = await secretService.SyncAsync(entity, idpEntity, cancellationToken);
        var allSecretsSynced = secretTargetStatuses.All(status => status.Synced);

        entity.Status.ObservedGeneration = entity.Metadata.Generation;
        entity.Status.ApplicationId = idpEntity.Status.ApplicationId;
        entity.Status.ClientId = idpEntity.Status.ClientId;
        entity.Status.LastRotatedAt = idpEntity.Status.LastRotatedAt;
        entity.Status.NextRotationAt = idpEntity.Status.NextRotationAt;
        entity.Status.SecretTargets = secretTargetStatuses;

        entity.UpsertCondition(
            entity.Status.Conditions,
            ConditionTypes.Ready,
            allSecretsSynced ? ConditionStatus.True : ConditionStatus.False,
            allSecretsSynced ? ConditionReasons.ReconcileSucceeded : ConditionReasons.SecretSyncFailed,
            allSecretsSynced
                ? "OIDC application has been reconciled with the identity provider."
                : "OIDC application was reconciled, but one or more target secrets failed to sync.");
        entity.UpsertCondition(
            entity.Status.Conditions,
            ConditionTypes.DependenciesReady,
            ConditionStatus.True,
            ConditionReasons.DependenciesResolved,
            "Referenced provider and credentials were resolved.");
        entity.UpsertCondition(
            entity.Status.Conditions,
            ConditionTypes.SecretsReady,
            allSecretsSynced ? ConditionStatus.True : ConditionStatus.False,
            allSecretsSynced ? ConditionReasons.SecretSyncSucceeded : ConditionReasons.SecretSyncFailed,
            allSecretsSynced
                ? "All target secrets were synced."
                : "One or more target secrets failed to sync.");
        entity.UpsertCondition(
            entity.Status.Conditions,
            ConditionTypes.RotationReady,
            ConditionStatus.True,
            idpEntity.Spec.RotateClientSecret ? ConditionReasons.RotationSucceeded : ConditionReasons.RotationNotDue,
            idpEntity.Spec.RotateClientSecret
                ? "OIDC client secret was rotated."
                : "OIDC client secret rotation is not due.");

        await client.UpdateStatusAsync(entity, cancellationToken);
        logger.LogDebug("Updated OIDC application status after reconcile with application id {ApplicationId}", idpEntity.Status.ApplicationId);

        entityRequeue(entity, TimeSpan.FromSeconds(20));
    }

    public async Task HandleExceptionAsync(OidcApplication entity, MoiraException exception, CancellationToken cancellationToken)
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
        logger.LogDebug("Updated OIDC application status after failed operation with reason {FailureReason}", exception.Reason);

        entityRequeue(entity, TimeSpan.FromSeconds(20));
    }

    public async Task HandleDeleteAsync(OidcApplication entity, IdPOidcApplication idpEntity, CancellationToken cancellationToken)
    {
        await secretService.DeleteAsync(entity, cancellationToken);
    }
}
