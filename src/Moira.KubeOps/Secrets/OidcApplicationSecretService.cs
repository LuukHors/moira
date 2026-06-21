using k8s.Models;
using Microsoft.Extensions.Logging;
using Moira.Common.Models;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.Secrets;

public class OidcApplicationSecretService(
    ISecretService secretService,
    ILogger<OidcApplicationSecretService> logger) : IOidcApplicationSecretService
{
    private const string ClientIdKey = "ClientId";
    private const string ClientSecretKey = "ClientSecret";

    public async Task<IEnumerable<OidcApplication.SecretTargetStatus>> SyncAsync(
        OidcApplication entity,
        IdPOidcApplication idpEntity,
        CancellationToken cancellationToken)
    {
        await secretService.SyncAsync(SourceTarget(entity, idpEntity), cancellationToken);

        var statuses = new List<OidcApplication.SecretTargetStatus>();
        foreach (var target in entity.Spec.SecretTargets)
        {
            try
            {
                var genericStatus = await secretService.SyncAsync(
                    ToSecretTarget(entity, target, idpEntity),
                    cancellationToken);

                statuses.Add(ToOidcStatus(genericStatus));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to sync OIDC application secret target {SecretNamespace}/{SecretName}", target.Namespace, target.Name);
                statuses.Add(ToFailedOidcStatus(target, ex));
            }
        }

        return statuses;
    }

    public async Task DeleteAsync(OidcApplication entity, CancellationToken cancellationToken)
    {
        await secretService.DeleteAsync(SourceTarget(entity), cancellationToken);

        foreach (var target in entity.Spec.SecretTargets)
        {
            await secretService.DeleteAsync(ToSecretTarget(entity, target), cancellationToken);
        }
    }

    private static SecretTarget SourceTarget(OidcApplication entity, IdPOidcApplication? idpEntity = null)
    {
        return new SecretTarget
        {
            Name = OidcSecretNames.SourceSecretName(entity),
            Namespace = entity.Namespace(),
            Type = "Opaque",
            Labels = new Dictionary<string, string>
            {
                ["moira.operator/managed"] = "true",
                ["moira.operator/oidc-application"] = entity.Name()
            },
            Data = idpEntity is null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string>
                {
                    [ClientIdKey] = idpEntity.Status.ClientId,
                    [ClientSecretKey] = idpEntity.ClientSecret
                }
        };
    }

    private static SecretTarget ToSecretTarget(
        OidcApplication entity,
        OidcApplication.SecretTarget target,
        IdPOidcApplication? idpEntity = null)
    {
        return new SecretTarget
        {
            Name = target.Name,
            Namespace = string.IsNullOrWhiteSpace(target.Namespace) ? entity.Namespace() : target.Namespace,
            ClusterRef = ToClusterRef(target.ClusterRef),
            Type = target.Type,
            Labels = target.Labels,
            Annotations = target.Annotations,
            Data = idpEntity is null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string>
                {
                    [target.Keys.ClientId] = idpEntity.Status.ClientId,
                    [target.Keys.ClientSecret] = idpEntity.ClientSecret
                }
        };
    }

    private static ClusterRef? ToClusterRef(OidcApplication.ClusterRef? clusterRef)
    {
        if (clusterRef is null)
        {
            return null;
        }

        return new ClusterRef
        {
            KubeConfigSecretRef = new KubeConfigSecretRef
            {
                Name = clusterRef.KubeConfigSecretRef.Name,
                Namespace = clusterRef.KubeConfigSecretRef.Namespace,
                Key = clusterRef.KubeConfigSecretRef.Key
            }
        };
    }

    private static OidcApplication.SecretTargetStatus ToOidcStatus(SecretTargetStatus status)
    {
        return new OidcApplication.SecretTargetStatus
        {
            Name = status.Name,
            Namespace = status.Namespace,
            Cluster = status.Cluster,
            LastSyncedAt = status.LastSyncedAt,
            Synced = status.Synced,
            Message = status.Message
        };
    }

    private static OidcApplication.SecretTargetStatus ToFailedOidcStatus(
        OidcApplication.SecretTarget target,
        Exception exception)
    {
        return new OidcApplication.SecretTargetStatus
        {
            Name = target.Name,
            Namespace = target.Namespace,
            Cluster = target.ClusterRef is null
                ? "local"
                : $"{target.ClusterRef.KubeConfigSecretRef.Namespace}/{target.ClusterRef.KubeConfigSecretRef.Name}",
            Synced = false,
            Message = exception.Message
        };
    }
}
