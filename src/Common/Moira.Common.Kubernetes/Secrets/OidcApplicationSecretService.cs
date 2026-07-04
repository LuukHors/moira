using k8s;
using k8s.Models;
using KubeOps.Abstractions.Entities;
using Microsoft.Extensions.Logging;
using Moira.Common.Abstractions.Models;
using Moira.Common.Kubernetes.Secrets.Models;

namespace Moira.Common.Kubernetes.Secrets;

public class OidcApplicationSecretService<TK8SEntity>(
    ISecretService secretService,
    ILogger<OidcApplicationSecretService<TK8SEntity>> logger) : IOidcApplicationSecretService<TK8SEntity> where TK8SEntity : CustomKubernetesEntity
{
    private const string ClientIdKey = "ClientId";
    private const string ClientSecretKey = "ClientSecret";
    private const string ClientIdToken = "{clientId}";
    private const string ClientSecretToken = "{clientSecret}";

    public async Task<IEnumerable<SecretStatus>> SyncAsync(
        TK8SEntity entity,
        OidcApplicationData data,
        IEnumerable<Secret> desiredSecrets,
        IEnumerable<SecretStatus> previousStatuses,
        CancellationToken cancellationToken)
    {
        await secretService.SyncAsync(ToSourceTargetSecret(entity, data), cancellationToken);

        var desiredTargets = desiredSecrets
            .Select(secret => ToSecret(entity, secret, data))
            .ToList();
        var desiredKeys = desiredTargets
            .Select(SecretTargetKey)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var statuses = new List<SecretStatus>();
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
                await secretService.DeleteAsync(staleTarget.Name, staleTarget.Namespace, staleTarget.ClusterRef, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete stale OIDC application secret {SecretNamespace}/{SecretName}", staleTarget.Namespace, staleTarget.Name);
                statuses.Add(ToFailedOidcStatus(staleTarget, ex, "Secret delete failed."));
            }
        }

        return statuses;
    }

    public async Task DeleteAsync(TK8SEntity entity, IEnumerable<Secret> secrets, CancellationToken cancellationToken)
    {
        await secretService.DeleteAsync(OidcSecretNames.SourceSecretName(entity), entity.Namespace(), clusterRef: null, cancellationToken);

        foreach (var secret in secrets)
        {
            await secretService.DeleteAsync(secret.Name, secret.Namespace, secret.ClusterRef, cancellationToken);
        }
    }
    
    private static SecretStatus ToFailedOidcStatus(
        Secret target,
        Exception exception,
        string messagePrefix = "Secret sync failed.")
    {
        return new SecretStatus
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

    private static Secret ToSourceTargetSecret(TK8SEntity entity, OidcApplicationData? data)
    {
        return new Secret
        {
            Name = OidcSecretNames.SourceSecretName(entity),
            Namespace = entity.Namespace(),
            Type = "Opaque",
            Labels = new Dictionary<string, string>
            {
                ["moira.operator/managed"] = "true",
                ["moira.operator/oidc-application"] = entity.Name()
            },
            Data = data is null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string>
                {
                    [ClientIdKey] = data.ClientId,
                    [ClientSecretKey] = data.ClientSecret
                }
        };
    }

    private static Secret ToSecret(
        TK8SEntity entity,
        Secret secret,
        OidcApplicationData? data)
    {
        var targetNamespace = string.IsNullOrWhiteSpace(secret.Namespace) ? entity.Namespace() : secret.Namespace;

        return new Secret
        {
            Name = secret.Name,
            Namespace = targetNamespace,
            ClusterRef = secret.ClusterRef,
            Type = secret.Type,
            Labels = secret.Labels,
            Annotations = secret.Annotations,
            Data = data is null
                ? new Dictionary<string, string>()
                : RenderData(secret, data)
        };
    }

    private static Dictionary<string, string> RenderData(
        Secret secret,
        OidcApplicationData data)
    {
        if (secret.Template.Count == 0)
        {
            return new Dictionary<string, string>
            {
                [ClientIdKey] = data.ClientId,
                [ClientSecretKey] = data.ClientSecret
            };
        }

        return secret.Template.ToDictionary(
            entry => entry.Key,
            entry => RenderTemplate(entry.Value, data));
    }

    private static string RenderTemplate(string template, OidcApplicationData data)
    {
        return template
            .Replace(ClientIdToken, data.ClientId, StringComparison.Ordinal)
            .Replace(ClientSecretToken, data.ClientSecret, StringComparison.Ordinal);
    }

    private static SecretStatus ToOidcStatus(SecretStatus status)
    {
        return new SecretStatus
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

    private static string SecretTargetKey(Secret target)
    {
        var cluster = target.ClusterRef is null
            ? "local"
            : $"remote:{target.ClusterRef.Namespace}/{target.ClusterRef.Name}/{target.ClusterRef.Key}";

        return $"{cluster}/{target.Namespace}/{target.Name}";
    }

    private static string SecretStatusKey(SecretStatus status)
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
        SecretStatus status,
        out Secret target)
    {
        target = new Secret
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

    private static ClusterSecretRef? ToOidcClusterRef(ClusterSecretRef? clusterRef)
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

    private static ClusterSecretRef? ToClusterRef(ClusterSecretRef? clusterRef)
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
