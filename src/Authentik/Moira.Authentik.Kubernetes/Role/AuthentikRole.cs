using k8s.Models;
using KubeOps.Abstractions.Entities;
using Moira.Authentik.Kubernetes.Provider;

namespace Moira.Authentik.Kubernetes.Role;

[KubernetesEntity(Group = "moira.operator", ApiVersion = "v1alpha1", Kind = "AuthentikRole")]
public class AuthentikRole : CustomKubernetesEntity<AuthentikRole.RoleSpec, AuthentikRole.RoleStatus>
{
    public class RoleSpec
    {
        public string DisplayName { get; set; } = string.Empty;
        
        public IList<string> Permissions { get; set; } = [];
        
        public ProviderRef ProviderRef { get; set; } = new();
    }

    public class RoleStatus
    {
        public string RoleId { get; set; } = string.Empty;
        public long? ObservedGeneration { get; set; } = 0;
        public IList<V1Condition> Conditions { get; set; } = new List<V1Condition>();
    }
}