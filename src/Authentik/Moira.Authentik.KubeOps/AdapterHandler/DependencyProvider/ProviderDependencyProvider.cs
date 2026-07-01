using System.Text;
using k8s.Models;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using Moira.Authentik.KubeOps.Entities;
using Moira.Common.Abstractions.Exceptions;
using Moira.Common.Abstractions.Models;
using Moira.Common.KubeOps.AdapterHandler.DependencyProvider;

namespace Moira.Authentik.KubeOps.AdapterHandler.DependencyProvider;

public class ProviderDependencyProvider(
    IKubernetesClient client,
    ILogger<ProviderDependencyProvider> logger) : IDependencyProvider<AuthentikProvider, IdPProvider>
{
    private const string ProviderType = "Authentik";

    public async Task<IdPProvider> ResolveAsync(AuthentikProvider entity, CancellationToken cancellationToken)
    {
        logger.LogDebug("Resolving secret {SecretNamespace}/{SecretName} for provider {ProviderName}", entity.Spec.SecretRef.NamespaceProperty, entity.Spec.SecretRef.Name, entity.Name());

        var secret = await client.GetAsync<V1Secret>(entity.Spec.SecretRef.Name, entity.Spec.SecretRef.NamespaceProperty, cancellationToken)
                     ?? throw new SecretNotFoundException(entity.Spec.SecretRef.NamespaceProperty, entity.Spec.SecretRef.Name);

        logger.LogDebug("Resolved secret {SecretNamespace}/{SecretName} for provider {ProviderName}, decoding secret values", entity.Spec.SecretRef.NamespaceProperty, entity.Spec.SecretRef.Name, entity.Name());

        var gotClientIdFromSecret = secret.Data.TryGetValue("ClientId", out var clientIdByteArray);
        var gotClientSecretFromSecret = secret.Data.TryGetValue("ClientSecret", out var clientSecretByteArray);

        if (!gotClientIdFromSecret || !gotClientSecretFromSecret || clientIdByteArray is null || clientSecretByteArray is null)
        {
            (bool Found, string Name)[] secretKeys =
            [
                (gotClientIdFromSecret && clientIdByteArray is not null, "ClientId"),
                (gotClientSecretFromSecret && clientSecretByteArray is not null, "ClientSecret")
            ];
            var missingKeys = secretKeys
                .Where(key => !key.Found)
                .Select(key => key.Name);

            throw new SecretKeyMissingException(entity.Spec.SecretRef.NamespaceProperty, entity.Spec.SecretRef.Name, missingKeys);
        }

        var clientId = Encoding.UTF8.GetString(clientIdByteArray);
        var clientSecret = Encoding.UTF8.GetString(clientSecretByteArray);

        logger.LogDebug("Decoded provider secret values for provider {ProviderName}", entity.Name());

        return new IdPProvider(
            entity.Namespace(),
            entity.Name(),
            ProviderType,
            entity.Spec.BaseUrl,
            clientId,
            clientSecret
        );
    }
}