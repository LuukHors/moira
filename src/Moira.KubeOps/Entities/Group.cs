using k8s.Models;
using KubeOps.Abstractions.Entities;

namespace Moira.KubeOps.Entities;

[KubernetesEntity(Group = "moira.operator", ApiVersion = "v1alpha1", Kind = "Group")]
public class Group : CustomKubernetesEntity<Group.GroupSpec, Group.GroupStatus>
{
    public class GroupSpec
    {
        public string DisplayName { get; set; } = string.Empty;
        public IEnumerable<string> MemberOf { get; set; } = [];
        public ProviderRef ProviderRef { get; set; } = new();
    }

    public class GroupStatus
    {
        public string GroupId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public IEnumerable<string> MemberOfGroupIds { get; set; } = [];
        public bool Synced { get; set; } = false;
        public long? ObservedGeneration { get; set; } = 0;
        public string ErrorMessage { get; set; } = string.Empty;
 
        public IList<V1Condition> Conditions { get; set; } = new List<V1Condition>();
    }
}