namespace Moira.KubeOps.Secrets;

public class KubeConfigSecretRef
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}
