using k8s.Models;
using KubeOps.Abstractions.Entities;

namespace Moira.Authentik.KubeOps.Entities;

[KubernetesEntity(Group = "moira.operator", ApiVersion = "v1alpha1", Kind = "AuthentikGroup")]
public class AuthentikGroup : CustomKubernetesEntity<AuthentikGroup.GroupSpec, AuthentikGroup.GroupStatus>
{
    public class GroupSpec
    {
        public string DisplayName { get; set; } = string.Empty;
        public IEnumerable<string> MemberOf { get; set; } = [];
        public ProviderRef ProviderRef { get; set; } = new();
        public bool AutoDelete { get; set; } = true;
        public IDictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
    }

    public class GroupStatus
    {
        public string GroupId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public IEnumerable<string> MemberOfGroupIds { get; set; } = [];
        public long? ObservedGeneration { get; set; } = 0;
        public IList<V1Condition> Conditions { get; set; } = new List<V1Condition>();
    }
}