using k8s.Models;
using KubeOps.Abstractions.Entities;

namespace Moira.Authentik.Kubernetes.Provider;

[KubernetesEntity(Group = "moira.operator", ApiVersion = "v1alpha1", Kind = "AuthentikProvider")]
public class AuthentikProvider : CustomKubernetesEntity<AuthentikProvider.ProviderSpec, AuthentikProvider.ProviderStatus>
{
    public class ProviderSpec
    {
        public string BaseUrl { get; set; } = string.Empty;
        
        public ManagedApplication ManagedApplication { get; set; }
        
        public V1SecretReference SecretRef { get; set; } = new();
    }

    public class ManagedApplication
    {
        public bool Enabled { get; init; } = false;
        public string DisplayName { get; init; } = string.Empty;
    }

    public class ProviderStatus
    {
        public long? ObservedGeneration { get; set; } = 0;
        public IList<V1Condition> Conditions { get; set; } = new List<V1Condition>();
    }
}