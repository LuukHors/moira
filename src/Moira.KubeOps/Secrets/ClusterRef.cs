namespace Moira.KubeOps.Secrets;

public class ClusterRef
{
    public KubeConfigSecretRef KubeConfigSecretRef { get; set; } = new();
}
