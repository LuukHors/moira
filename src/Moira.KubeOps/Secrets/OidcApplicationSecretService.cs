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
    private const string ClientIdToken = "{clientId}";
    private const string ClientSecretToken = "{clientSecret}";

    public async Task<IEnumerable<OidcApplication.SecretStatus>> SyncAsync(
        OidcApplication entity,
        IdPOidcApplication idpEntity,
        CancellationToken cancellationToken)
    {
        await secretService.SyncAsync(SourceTarget(entity, idpEntity), cancellationToken);

        var statuses = new List<OidcApplication.SecretStatus>();
        foreach (var secret in entity.Spec.Secrets ?? [])
        {
            try
            {
                var genericStatus = await secretService.SyncAsync(
                    ToSecretTarget(entity, secret, idpEntity),
                    cancellationToken);

                statuses.Add(ToOidcStatus(genericStatus));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to sync OIDC application secret {SecretNamespace}/{SecretName}", secret.Namespace, secret.Name);
                statuses.Add(ToFailedOidcStatus(secret, ex));
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
        return new SecretTarget
        {
            Name = secret.Name,
            Namespace = string.IsNullOrWhiteSpace(secret.Namespace) ? entity.Namespace() : secret.Namespace,
            ClusterRef = ToClusterRef(secret.ClusterRef),
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

    private static OidcApplication.SecretStatus ToOidcStatus(SecretTargetStatus status)
    {
        return new OidcApplication.SecretStatus
        {
            Name = status.Name,
            Namespace = status.Namespace,
            Cluster = status.Cluster,
            LastSyncedAt = status.LastSyncedAt,
            Synced = status.Synced,
            Message = status.Message
        };
    }

    private static OidcApplication.SecretStatus ToFailedOidcStatus(
        OidcApplication.Secret secret,
        Exception exception)
    {
        return new OidcApplication.SecretStatus
        {
            Name = secret.Name,
            Namespace = secret.Namespace,
            Cluster = secret.ClusterRef is null
                ? "local"
                : $"{secret.ClusterRef.KubeConfigSecretRef.Namespace}/{secret.ClusterRef.KubeConfigSecretRef.Name}",
            Synced = false,
            Message = exception.Message
        };
    }
}
