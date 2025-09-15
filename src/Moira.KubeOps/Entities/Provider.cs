using k8s.Models;
using KubeOps.Abstractions.Entities;

namespace Moira.KubeOps.Entities;

[KubernetesEntity(Group = "moira.operator", ApiVersion = "v1alpha1", Kind = "Provider")]
public class Provider : CustomKubernetesEntity<Provider.ProviderSpec, Provider.ProviderStatus>
{
    public class ProviderSpec
    {
        public ProviderType Type { get; set; }
    }

    public class ProviderStatus
    {
        
    }
}