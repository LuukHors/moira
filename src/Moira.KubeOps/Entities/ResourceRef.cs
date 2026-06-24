namespace Moira.KubeOps.Entities;

public class ResourceRef
{
    public string ApiVersion { get; set; } = "moira.operator/v1alpha1";
    public string Kind { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
}
