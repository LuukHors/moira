namespace Moira.KubeOps.Secrets.Models;

public class ClusterSecretRef
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}
