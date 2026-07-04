using System.Text;
using k8s;
using k8s.Autorest;
using k8s.Models;
using KubeOps.Abstractions.Entities;
using KubeOps.KubernetesClient;
using Moira.Common.Kubernetes.Secrets.Models;

namespace Moira.Common.Kubernetes.Secrets;

public class SecretService(IKubernetesClient client) : ISecretService
{
    public async Task<SecretStatus> SyncAsync(Secret target, CancellationToken cancellationToken)
    {
        var status = TargetStatus(target);
        if (target.ClusterRef is null)
        {
            await UpsertLocalSecretAsync(target, cancellationToken);
        }
        else
        {
            await UpsertRemoteSecretAsync(target, cancellationToken);
        }

        status.Synced = true;
        status.LastSyncedAt = DateTime.UtcNow;
        status.Message = "Secret synced.";
        return status;
    }

    public async Task DeleteAsync(string secretName, string secretNamespace, ClusterSecretRef? clusterRef, CancellationToken cancellationToken)
    {
        if (clusterRef is null)
        {
            await DeleteLocalSecretAsync(secretName, secretNamespace, cancellationToken);
            return;
        }

        await DeleteRemoteSecretAsync(secretName, secretNamespace, clusterRef, cancellationToken);
    }

    private async Task UpsertLocalSecretAsync(Secret target, CancellationToken cancellationToken)
    {
        var secret = BuildSecret(target);
        await client.SaveAsync(secret, cancellationToken);
    }

    private async Task UpsertRemoteSecretAsync(Secret target, CancellationToken cancellationToken)
    {
        using var remoteClient = await BuildRemoteClientAsync(target.ClusterRef!, cancellationToken);
        var secret = BuildSecret(target);

        try
        {
            var existing = await remoteClient.ReadNamespacedSecretAsync(target.Name, target.Namespace, cancellationToken: cancellationToken);
            secret.Metadata.ResourceVersion = existing.Metadata.ResourceVersion;
            await remoteClient.ReplaceNamespacedSecretAsync(secret, target.Name, target.Namespace, cancellationToken: cancellationToken);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            await remoteClient.CreateNamespacedSecretAsync(secret, target.Namespace, cancellationToken: cancellationToken);
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

    private async Task DeleteRemoteSecretAsync(string secretName, string secretNamespace, ClusterSecretRef clusterRef, CancellationToken cancellationToken)
    {
        using var remoteClient = await BuildRemoteClientAsync(clusterRef, cancellationToken);
        try
        {
            await remoteClient.DeleteNamespacedSecretAsync(secretName, secretNamespace, cancellationToken: cancellationToken);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
        }
    }

    private async Task<k8s.Kubernetes> BuildRemoteClientAsync(
        ClusterSecretRef clusterSecretRef,
        CancellationToken cancellationToken)
    {
        var kubeConfigSecret = await client.GetAsync<V1Secret>(
            clusterSecretRef.Name,
            clusterSecretRef.Namespace,
            cancellationToken);

        if (kubeConfigSecret?.Data is null ||
            !kubeConfigSecret.Data.TryGetValue(clusterSecretRef.Key, out var kubeConfigBytes))
        {
            throw new InvalidOperationException(
                $"Kubeconfig secret key '{clusterSecretRef.Namespace}/{clusterSecretRef.Name}:{clusterSecretRef.Key}' was not found.");
        }

        await using var stream = new MemoryStream(kubeConfigBytes);
        var config = await KubernetesClientConfiguration.BuildConfigFromConfigFileAsync(stream);
        return new k8s.Kubernetes(config);
    }

    private static V1Secret BuildSecret(Secret target)
    {
        var secret = new V1Secret
        {
            Metadata = new V1ObjectMeta
            {
                Name = target.Name,
                NamespaceProperty = target.Namespace,
                Annotations = new Dictionary<string, string>(target.Annotations),
                Labels = new Dictionary<string, string>(target.Labels)
            },
            Type = string.IsNullOrWhiteSpace(target.Type) ? "Opaque" : target.Type,
            Data = target.Data.ToDictionary(
                entry => entry.Key,
                entry => Encoding.UTF8.GetBytes(entry.Value))
        };

        // if (target.Owner is not null)
        // {
        //     secret.WithOwnerReference(target.Owner);
        // }

        return secret;
    }

    private static SecretStatus TargetStatus(Secret target)
    {
        return new SecretStatus
        {
            Name = target.Name,
            Namespace = target.Namespace,
            Cluster = target.ClusterRef is null
                ? "local"
                : $"{target.ClusterRef.Namespace}/{target.ClusterRef.Name}",
            ClusterRef = target.ClusterRef
        };
    }
}
