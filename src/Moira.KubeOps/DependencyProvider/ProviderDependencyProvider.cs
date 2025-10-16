using System.Text;
using k8s.Models;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using Moira.Common.Models;
using Moira.Common.RequestContext;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.DependencyProvider;

public class ProviderDependencyProvider(
    IKubernetesClient client,
    IRequestContextProvider requestContext,
    ILogger<ProviderDependencyProvider> logger) : IDependencyProvider<Provider, IdPProvider>
{
    public async Task<IdPProvider> ResolveAsync(Provider entity, CancellationToken cancellationToken)
    {
        logger.LogDebug("[{commandId}][IdPGroup][{entityName}] Getting secret {secretNamespace}/{secretName} for provider {providerName}", requestContext.RequestId, entity.Name(), entity.Spec.SecretRef.Name, entity.Spec.SecretRef.Name, entity.Name());
        
        var secret = await client.GetAsync<V1Secret>(entity.Spec.SecretRef.Name, entity.Spec.SecretRef.NamespaceProperty, cancellationToken) 
                     ?? throw new InvalidOperationException($"Could not get secret {entity.Spec.SecretRef.NamespaceProperty}/{entity.Spec.SecretRef.Name}");
        
        logger.LogDebug("[{commandId}][IdPGroup][{entityName}] Received secret {secretNamespace}/{secretName} for provider {providerName}, Decoding secret values...", requestContext.RequestId, entity.Name(), entity.Spec.SecretRef.Name, entity.Spec.SecretRef.Name, entity.Name());

        var gotClientIdFromSecret = secret.Data.TryGetValue("ClientId", out var clientIdByteArray);
        var gotclientSecretFromSecret = secret.Data.TryGetValue("ClientSecret", out var clientSecretByteArray);

        if (!gotClientIdFromSecret || !gotclientSecretFromSecret || clientIdByteArray is null || clientSecretByteArray is null)
        {
            throw new InvalidOperationException($"Could not get key/value ClientId or ClientSecret from secret {entity.Spec.SecretRef.NamespaceProperty}/{entity.Spec.SecretRef.Name}");
        }
        
        var clientId = Encoding.UTF8.GetString(clientIdByteArray);
        var clientSecret = Encoding.UTF8.GetString(clientSecretByteArray);
        
        logger.LogDebug("[{commandId}][IdPGroup][{entityName}] Got secret values, returning IdPProvider", requestContext.RequestId, entity.Name());

        return new IdPProvider(
            entity.Namespace(),
            entity.Name(),
            entity.Spec.Type.ToString(),
            entity.Spec.BaseUrl,
            clientId,
            clientSecret
        );
    }
}