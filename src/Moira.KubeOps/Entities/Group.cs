using k8s.Models;
using KubeOps.Abstractions.Entities;

namespace Moira.KubeOps.Entities;

[KubernetesEntity(Group = "moira.operator", ApiVersion = "v1alpha1", Kind = "Group")]
public class Group : CustomKubernetesEntity<Group.GroupSpec, Group.GroupStatus>
{
    public class GroupSpec
    {
        public string DisplayName { get; set; }
        public IEnumerable<string> MemberOf { get; set; } = [];
        public ProviderRef ProviderRef { get; set; }
    }

    public class GroupStatus
    {
        public string GroupId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool Synced { get; set; } = false;
        public long? ObservedGeneration { get; set; } = 0;
        //to be implemented.
        public IList<V1Condition> Conditions { get; set; } = Array.Empty<V1Condition>();
    }
}