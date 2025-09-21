using System.Text;
using k8s.Models;
using KubeOps.KubernetesClient;
using Moira.Common.Models;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.DependencyProvider;

public class ProviderDependencyProvider(
    IKubernetesClient client) : IDependencyProvider<Provider, IdPProvider>
{
    public async Task<IdPProvider> ResolveAsync(Provider entity, CancellationToken cancellationToken)
    {
        var secret = await client.GetAsync<V1Secret>(entity.Spec.SecretRef.Name, entity.Spec.SecretRef.NamespaceProperty, cancellationToken) 
                     ?? throw new InvalidOperationException($"Could not get secret {entity.Spec.SecretRef.NamespaceProperty}/{entity.Spec.SecretRef.Name}");
        
        var gotClientIdFromSecret = secret.Data.TryGetValue("ClientId", out var clientIdByteArray);
        var gotclientSecretFromSecret = secret.Data.TryGetValue("ClientSecret", out var clientSecretByteArray);

        if (!gotClientIdFromSecret || !gotclientSecretFromSecret || clientIdByteArray is null || clientSecretByteArray is null)
        {
            throw new InvalidOperationException($"Could not get key/value ClientId or ClientSecret from secret {entity.Spec.SecretRef.NamespaceProperty}/{entity.Spec.SecretRef.Name}");
        }
        
        var clientId = Encoding.UTF8.GetString(clientIdByteArray);
        var clientSecret = Encoding.UTF8.GetString(clientSecretByteArray);
        
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