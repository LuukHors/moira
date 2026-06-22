using System.Text;
using k8s;
using k8s.Autorest;
using k8s.Models;
using KubeOps.Abstractions.Entities;
using KubeOps.KubernetesClient;

namespace Moira.KubeOps.Secrets;

public class SecretService(IKubernetesClient client) : ISecretService
{
    public async Task<SecretTargetStatus> SyncAsync(SecretTarget target, CancellationToken cancellationToken)
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

    public async Task DeleteAsync(SecretTarget target, CancellationToken cancellationToken)
    {
        if (target.ClusterRef is null)
        {
            await DeleteLocalSecretAsync(target.Name, target.Namespace, cancellationToken);
            return;
        }

        await DeleteRemoteSecretAsync(target, cancellationToken);
    }

    private async Task UpsertLocalSecretAsync(SecretTarget target, CancellationToken cancellationToken)
    {
        var secret = BuildSecret(target);
        await client.SaveAsync(secret, cancellationToken);
    }

    private async Task UpsertRemoteSecretAsync(SecretTarget target, CancellationToken cancellationToken)
    {
        using var remoteClient = await BuildRemoteClientAsync(target.ClusterRef!.KubeConfigSecretRef, cancellationToken);
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

    private async Task DeleteRemoteSecretAsync(SecretTarget target, CancellationToken cancellationToken)
    {
        using var remoteClient = await BuildRemoteClientAsync(target.ClusterRef!.KubeConfigSecretRef, cancellationToken);
        try
        {
            await remoteClient.DeleteNamespacedSecretAsync(target.Name, target.Namespace, cancellationToken: cancellationToken);
        }
        catch (HttpOperationException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
        }
    }

    private async Task<Kubernetes> BuildRemoteClientAsync(
        KubeConfigSecretRef kubeConfigSecretRef,
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

    private static V1Secret BuildSecret(SecretTarget target)
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

        if (target.Owner is not null)
        {
            secret.WithOwnerReference(target.Owner);
        }

        return secret;
    }

    private static SecretTargetStatus TargetStatus(SecretTarget target)
    {
        return new SecretTargetStatus
        {
            Name = target.Name,
            Namespace = target.Namespace,
            Cluster = target.ClusterRef is null
                ? "local"
                : $"{target.ClusterRef.KubeConfigSecretRef.Namespace}/{target.ClusterRef.KubeConfigSecretRef.Name}",
            ClusterRef = target.ClusterRef
        };
    }
}
