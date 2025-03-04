using k8s.Models;
using KubeOps.Abstractions.Entities;

namespace IDPOperator.KubeOps.Controllers;

[KubernetesEntity(Group = "testing.dev", ApiVersion = "v1", Kind = "V1TestEntity")]
public class V1TestEntity : CustomKubernetesEntity
{
    public string ApiVersion { get; set; } = "v1";
    public string Kind { get; set; } = "V1TestEntity";
    public V1ObjectMeta Metadata { get; set; } = new();
}