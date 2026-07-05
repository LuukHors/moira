using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using Moira.Authentik.Application.Models;
using Moira.Authentik.Application.Models.OidcApplication;
using Moira.Common.Abstractions.Exceptions;
using Moira.Common.Kubernetes.Mappers;
using Moira.Common.Kubernetes.ResultHandler;
using Moira.Common.Kubernetes.Secrets;
using Moira.Common.Kubernetes.Secrets.Models;
using Moira.Common.Kubernetes.Status;

namespace Moira.Authentik.Kubernetes.OidcApplication;

public class OidcApplicationResultHandler(
    IKubernetesClient client,
    IOidcApplicationSecretService<AuthentikOidcApplication> secretService,
    ILogger<OidcApplicationResultHandler> logger) : IResultHandler<AuthentikOidcApplication, AuthentikOidcApplicationModel>
{
    public async Task HandleReconcileResultAsync(AuthentikOidcApplication entity, AuthentikOidcApplicationModel idpEntity, CancellationToken cancellationToken)
    {
        var secretTargetStatuses = await secretService.SyncAsync(entity, ToOidcApplicationData(idpEntity),
            entity.Spec.Secrets, entity.Status.Secrets, cancellationToken);
        var allSecretsSynced = secretTargetStatuses.All(status => status.Synced);

        entity.Status.ObservedGeneration = entity.Metadata.Generation;
        entity.Status.ApplicationId = idpEntity.Status.ApplicationId;
        entity.Status.ClientId = idpEntity.Status.ClientId;
        entity.Status.ProviderResourceIds = idpEntity.Status.ProviderResourceIds;
        entity.Status.LastRotatedAt = idpEntity.Status.LastRotatedAt;
        entity.Status.NextRotationAt = idpEntity.Status.NextRotationAt;
        entity.Status.Secrets = secretTargetStatuses;

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

        await client.UpdateStatusAsync(entity, cancellationToken);
        logger.LogDebug("Updated OIDC application status after reconcile with application id {ApplicationId}", idpEntity.Status.ApplicationId);
    }

    public async Task HandleExceptionAsync(AuthentikOidcApplication entity, MoiraException exception, CancellationToken cancellationToken)
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
    }

    public async Task HandleDeletedAsync(AuthentikOidcApplication entity, AuthentikOidcApplicationModel idpEntity, CancellationToken cancellationToken)
    {
        await secretService.DeleteAsync(entity, entity.Spec.Secrets, cancellationToken);
    }

    private static OidcApplicationData ToOidcApplicationData(AuthentikOidcApplicationModel entity)
    {
        return new OidcApplicationData(entity.Status.ClientId, entity.ClientSecret);
    }
}
