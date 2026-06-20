using System.Text;
using k8s;
using k8s.Autorest;
using k8s.Models;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using Moira.Common.Models;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.Secrets;

public class OidcApplicationSecretService(
    IKubernetesClient client,
    ILogger<OidcApplicationSecretService> logger) : IOidcApplicationSecretService
{
    private const string ClientIdKey = "clientId";
    private const string ClientSecretKey = "clientSecret";

    public async Task<IEnumerable<OidcApplication.SecretTargetStatus>> SyncAsync(
        OidcApplication entity,
        IdPOidcApplication idpEntity,
        CancellationToken cancellationToken)
    {
        await UpsertLocalSecretAsync(
            OidcSecretNames.SourceSecretName(entity),
            entity.Namespace(),
            "Opaque",
            new Dictionary<string, string>(),
            new Dictionary<string, string>
            {
                ["moira.operator/managed"] = "true",
                ["moira.operator/oidc-application"] = entity.Name()
            },
            ClientIdKey,
            ClientSecretKey,
            idpEntity.Status.ClientId,
            idpEntity.ClientSecret,
            cancellationToken);

        var statuses = new List<OidcApplication.SecretTargetStatus>();
        foreach (var target in entity.Spec.SecretTargets)
        {
            var status = TargetStatus(target);
            try
            {
                var targetNamespace = string.IsNullOrWhiteSpace(target.Namespace) ? entity.Namespace() : target.Namespace;
                if (target.ClusterRef is null)
                {
                    await UpsertLocalSecretAsync(
                        target.Name,
                        targetNamespace,
                        target.Type,
                        target.Annotations,
                        target.Labels,
                        target.Keys.ClientId,
                        target.Keys.ClientSecret,
                        idpEntity.Status.ClientId,
                        idpEntity.ClientSecret,
                        cancellationToken);
                }
                else
                {
                    await UpsertRemoteSecretAsync(
                        target,
                        targetNamespace,
                        idpEntity.Status.ClientId,
                        idpEntity.ClientSecret,
                        cancellationToken);
                }

                status.Synced = true;
                status.LastSyncedAt = DateTime.UtcNow;
                status.Message = "Secret synced.";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to sync OIDC application secret target {SecretNamespace}/{SecretName}", target.Namespace, target.Name);
                status.Synced = false;
                status.Message = ex.Message;
            }

            statuses.Add(status);
        }

        return statuses;
    }

    public async Task DeleteAsync(OidcApplication entity, CancellationToken cancellationToken)
    {
        await DeleteLocalSecretAsync(OidcSecretNames.SourceSecretName(entity), entity.Namespace(), cancellationToken);

        foreach (var target in entity.Spec.SecretTargets)
        {
            var targetNamespace = string.IsNullOrWhiteSpace(target.Namespace) ? entity.Namespace() : target.Namespace;
            if (target.ClusterRef is null)
            {
                await DeleteLocalSecretAsync(target.Name, targetNamespace, cancellationToken);
                continue;
            }

            await DeleteRemoteSecretAsync(target, targetNamespace, cancellationToken);
        }
    }

    private async Task UpsertLocalSecretAsync(
        string name,
        string @namespace,
        string type,
        IDictionary<string, string> annotations,
        IDictionary<string, string> labels,
        string clientIdKey,
        string clientSecretKey,
        string clientId,
        string clientSecret,
        CancellationToken cancellationToken)
    {
        var secret = BuildSecret(name, @namespace, type, annotations, labels, clientIdKey, clientSecretKey, clientId, clientSecret);
        var existing = await client.GetAsync<V1Secret>(name, @namespace, cancellationToken);
        if (existing is null)
        {
            await client.CreateAsync(secret, cancellationToken);
            return;
        }

        secret.Metadata.ResourceVersion = existing.Metadata.ResourceVersion;
        await client.UpdateAsync(secret, cancellationToken);
    }

    private async Task UpsertRemoteSecretAsync(
        OidcApplication.SecretTarget target,
        string targetNamespace,
        string clientId,
        string clientSecret,
        CancellationToken cancellationToken)
    {
        using var remoteClient = await BuildRemoteClientAsync(target.ClusterRef!.KubeConfigSecretRef, cancellationToken);
        var secret = BuildSecret(
            target.Name,
            targetNamespace,
            target.Type,
            target.Annotations,
            target.Labels,
            target.Keys.ClientId,
            target.Keys.ClientSecret,
            clientId,
            clientSecret);

        try
        {
            var existing = await remoteClient.ReadNamespacedSecretAsync(target.Name, targetNamespace, cancellationToken: cancellationToken);
            secret.Metadata.ResourceVersion = existing.Metadata.ResourceVersion;
            await remoteClient.ReplaceNamespacedSecretAsync(secret, target.Name, targetNamespace, cancellationToken: cancellationToken);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            await remoteClient.CreateNamespacedSecretAsync(secret, targetNamespace, cancellationToken: cancellationToken);
        }
    }

    private async Task DeleteLocalSecretAsync(string name, string @namespace, CancellationToken cancellationToken)
    {
        var existing = await client.GetAsync<V1Secret>(name, @namespace, cancellationToken);
        if (existing is not null)
        {
            await client.DeleteAsync<V1Secret>(name, @namespace, cancellationToken);
        }
    }

    private async Task DeleteRemoteSecretAsync(
        OidcApplication.SecretTarget target,
        string targetNamespace,
        CancellationToken cancellationToken)
    {
        using var remoteClient = await BuildRemoteClientAsync(target.ClusterRef!.KubeConfigSecretRef, cancellationToken);
        try
        {
            await remoteClient.DeleteNamespacedSecretAsync(target.Name, targetNamespace, cancellationToken: cancellationToken);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
        }
    }

    private async Task<Kubernetes> BuildRemoteClientAsync(
        OidcApplication.KubeConfigSecretRef kubeConfigSecretRef,
        CancellationToken cancellationToken)
    {
        var kubeConfigSecret = await client.GetAsync<V1Secret>(
            kubeConfigSecretRef.Name,
            kubeConfigSecretRef.Namespace,
            cancellationToken);

        if (kubeConfigSecret?.Data is null ||
            !kubeConfigSecret.Data.TryGetValue(kubeConfigSecretRef.Key, out var kubeConfigBytes))
        {
            throw new InvalidOperationException(
                $"Kubeconfig secret key '{kubeConfigSecretRef.Namespace}/{kubeConfigSecretRef.Name}:{kubeConfigSecretRef.Key}' was not found.");
        }

        await using var stream = new MemoryStream(kubeConfigBytes);
        var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(stream);
        return new Kubernetes(config);
    }

    private static V1Secret BuildSecret(
        string name,
        string @namespace,
        string type,
        IDictionary<string, string> annotations,
        IDictionary<string, string> labels,
        string clientIdKey,
        string clientSecretKey,
        string clientId,
        string clientSecret)
    {
        return new V1Secret
        {
            Metadata = new V1ObjectMeta
            {
                Name = name,
                NamespaceProperty = @namespace,
                Annotations = new Dictionary<string, string>(annotations),
                Labels = new Dictionary<string, string>(labels)
            },
            Type = string.IsNullOrWhiteSpace(type) ? "Opaque" : type,
            Data = new Dictionary<string, byte[]>
            {
                [clientIdKey] = Encoding.UTF8.GetBytes(clientId),
                [clientSecretKey] = Encoding.UTF8.GetBytes(clientSecret)
            }
        };
    }

    private static OidcApplication.SecretTargetStatus TargetStatus(OidcApplication.SecretTarget target)
    {
        return new OidcApplication.SecretTargetStatus
        {
            Name = target.Name,
            Namespace = target.Namespace,
            Cluster = target.ClusterRef is null
                ? "local"
                : $"{target.ClusterRef.KubeConfigSecretRef.Namespace}/{target.ClusterRef.KubeConfigSecretRef.Name}"
        };
    }
}
