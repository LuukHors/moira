using k8s.Models;
using Microsoft.Extensions.Logging;
using Moira.Common.Models;
using Moira.KubeOps.Entities;
using Moira.KubeOps.Secrets.Models;

namespace Moira.KubeOps.Secrets;

public class OidcApplicationSecretService(
    ISecretService secretService,
    ILogger<OidcApplicationSecretService> logger) : IOidcApplicationSecretService
{
    private const string ClientIdKey = "ClientId";
    private const string ClientSecretKey = "ClientSecret";
    private const string ClientIdToken = "{clientId}";
    private const string ClientSecretToken = "{clientSecret}";

    public async Task<IEnumerable<OidcApplication.SecretStatus>> SyncAsync(
        OidcApplication entity,
        IdPOidcApplication idpEntity,
        CancellationToken cancellationToken)
    {
        await secretService.SyncAsync(SourceTarget(entity, idpEntity), cancellationToken);

        var previousStatuses = entity.Status.Secrets ?? [];
        var desiredTargets = (entity.Spec.Secrets ?? [])
            .Select(secret => ToSecretTarget(entity, secret, idpEntity))
            .ToList();
        var desiredKeys = desiredTargets
            .Select(SecretTargetKey)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var statuses = new List<OidcApplication.SecretStatus>();
        foreach (var target in desiredTargets)
        {
            try
            {
                var genericStatus = await secretService.SyncAsync(target, cancellationToken);

                statuses.Add(ToOidcStatus(genericStatus));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to sync OIDC application secret {SecretNamespace}/{SecretName}", target.Namespace, target.Name);
                statuses.Add(ToFailedOidcStatus(target, ex));
            }
        }

        foreach (var previousStatus in previousStatuses)
        {
            if (desiredKeys.Contains(SecretStatusKey(previousStatus)))
            {
                continue;
            }

            if (!TryToSecretTarget(previousStatus, out var staleTarget))
            {
                continue;
            }

            try
            {
                await secretService.DeleteAsync(staleTarget, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete stale OIDC application secret {SecretNamespace}/{SecretName}", staleTarget.Namespace, staleTarget.Name);
                statuses.Add(ToFailedOidcStatus(staleTarget, ex, "Secret delete failed."));
            }
        }

        return statuses;
    }

    public async Task DeleteAsync(OidcApplication entity, CancellationToken cancellationToken)
    {
        await secretService.DeleteAsync(SourceTarget(entity), cancellationToken);

        foreach (var secret in entity.Spec.Secrets ?? [])
        {
            await secretService.DeleteAsync(ToSecretTarget(entity, secret), cancellationToken);
        }
    }

    private static SecretTarget SourceTarget(OidcApplication entity, IdPOidcApplication? idpEntity = null)
    {
        return new SecretTarget
        {
            Name = OidcSecretNames.SourceSecretName(entity),
            Namespace = entity.Namespace(),
            Owner = OwnerReferenceFor(entity, entity.Namespace(), null),
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
        OidcApplication.Secret secret,
        IdPOidcApplication? idpEntity = null)
    {
        var targetNamespace = string.IsNullOrWhiteSpace(secret.Namespace) ? entity.Namespace() : secret.Namespace;
        var clusterRef = ToClusterRef(secret.ClusterSecretRef);

        return new SecretTarget
        {
            Name = secret.Name,
            Namespace = targetNamespace,
            ClusterRef = clusterRef,
            Owner = OwnerReferenceFor(entity, targetNamespace, clusterRef),
            Type = secret.Type,
            Labels = secret.Labels,
            Annotations = secret.Annotations,
            Data = idpEntity is null
                ? new Dictionary<string, string>()
                : RenderData(secret, idpEntity)
        };
    }

    private static IDictionary<string, string> RenderData(
        OidcApplication.Secret secret,
        IdPOidcApplication idpEntity)
    {
        if (secret.Template is null || secret.Template.Count == 0)
        {
            return new Dictionary<string, string>
            {
                [ClientIdKey] = idpEntity.Status.ClientId,
                [ClientSecretKey] = idpEntity.ClientSecret
            };
        }

        return secret.Template.ToDictionary(
            entry => entry.Key,
            entry => RenderTemplate(entry.Value, idpEntity));
    }

    private static string RenderTemplate(string template, IdPOidcApplication idpEntity)
    {
        return template
            .Replace(ClientIdToken, idpEntity.Status.ClientId, StringComparison.Ordinal)
            .Replace(ClientSecretToken, idpEntity.ClientSecret, StringComparison.Ordinal);
    }

    private static OidcApplication.SecretStatus ToOidcStatus(SecretTargetStatus status)
    {
        return new OidcApplication.SecretStatus
        {
            Name = status.Name,
            Namespace = status.Namespace,
            Cluster = status.Cluster,
            ClusterRef = ToOidcClusterRef(status.ClusterRef),
            LastSyncedAt = status.LastSyncedAt,
            Synced = status.Synced,
            Message = status.Message
        };
    }

    private static OidcApplication.SecretStatus ToFailedOidcStatus(
        SecretTarget target,
        Exception exception,
        string messagePrefix = "Secret sync failed.")
    {
        return new OidcApplication.SecretStatus
        {
            Name = target.Name,
            Namespace = target.Namespace,
            Cluster = target.ClusterRef is null
                ? "local"
                : $"{target.ClusterRef.Namespace}/{target.ClusterRef.Name}",
            ClusterRef = ToOidcClusterRef(target.ClusterRef),
            Synced = false,
            Message = $"{messagePrefix} {exception.Message}"
        };
    }

    private static OidcApplication? OwnerReferenceFor(
        OidcApplication entity,
        string targetNamespace,
        ClusterSecretRef? clusterRef)
    {
        if (clusterRef is not null ||
            !string.Equals(targetNamespace, entity.Namespace(), StringComparison.Ordinal) ||
            string.IsNullOrWhiteSpace(entity.Metadata.Uid))
        {
            return null;
        }

        return entity;
    }

    private static string SecretTargetKey(SecretTarget target)
    {
        var cluster = target.ClusterRef is null
            ? "local"
            : $"remote:{target.ClusterRef.Namespace}/{target.ClusterRef.Name}/{target.ClusterRef.Key}";

        return $"{cluster}/{target.Namespace}/{target.Name}";
    }

    private static string SecretStatusKey(OidcApplication.SecretStatus status)
    {
        if (status.ClusterRef is not null)
        {
            return $"remote:{status.ClusterRef.Namespace}/{status.ClusterRef.Name}/{status.ClusterRef.Key}/{status.Namespace}/{status.Name}";
        }

        if (string.IsNullOrWhiteSpace(status.Cluster) ||
            string.Equals(status.Cluster, "local", StringComparison.OrdinalIgnoreCase))
        {
            return $"local/{status.Namespace}/{status.Name}";
        }

        return $"remote:{status.Cluster}/{status.Namespace}/{status.Name}";
    }

    private static bool TryToSecretTarget(
        OidcApplication.SecretStatus status,
        out SecretTarget target)
    {
        target = new SecretTarget
        {
            Name = status.Name,
            Namespace = status.Namespace,
            ClusterRef = ToClusterRef(status.ClusterRef)
        };

        if (target.ClusterRef is not null ||
            string.IsNullOrWhiteSpace(status.Cluster) ||
            string.Equals(status.Cluster, "local", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static OidcApplication.ClusterSecretRef? ToOidcClusterRef(ClusterSecretRef? clusterRef)
    {
        if (clusterRef is null)
        {
            return null;
        }

        return new OidcApplication.ClusterSecretRef
        { 
            Name = clusterRef.Name,
            Namespace = clusterRef.Namespace,
            Key = clusterRef.Key
        };
    }

    private static ClusterSecretRef? ToClusterRef(OidcApplication.ClusterSecretRef? clusterRef)
    {
        if (clusterRef is null)
        {
            return null;
        }

        return new ClusterSecretRef
        {
            Name = clusterRef.Name,
            Namespace = clusterRef.Namespace,
            Key = clusterRef.Key
        };
    }
}
