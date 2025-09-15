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
        
    }
}