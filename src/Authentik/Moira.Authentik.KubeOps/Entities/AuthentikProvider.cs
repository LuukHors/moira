using k8s.Models;
using KubeOps.Abstractions.Entities;

namespace Moira.Authentik.KubeOps.Entities;

[KubernetesEntity(Group = "moira.operator", ApiVersion = "v1alpha1", Kind = "AuthentikProvider")]
public class AuthentikProvider : CustomKubernetesEntity<AuthentikProvider.ProviderSpec, AuthentikProvider.ProviderStatus>
{
    public class ProviderSpec
    {
        public string BaseUrl { get; set; } = string.Empty;

        public V1SecretReference SecretRef { get; set; } = new();
    }

    public class ProviderStatus
    {
        public long? ObservedGeneration { get; set; } = 0;
        public IList<V1Condition> Conditions { get; set; } = new List<V1Condition>();
    }
}