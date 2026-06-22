using k8s;
using k8s.Models;

namespace Moira.KubeOps.Secrets;

public class SecretTarget
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public ClusterRef? ClusterRef { get; set; }
    public IKubernetesObject<V1ObjectMeta>? Owner { get; set; }
    public string Type { get; set; } = "Opaque";
    public IDictionary<string, string> Labels { get; set; } = new Dictionary<string, string>();
    public IDictionary<string, string> Annotations { get; set; } = new Dictionary<string, string>();
    public IDictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
}
