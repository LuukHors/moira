namespace Moira.KubeOps.Entities;

public class ProviderRef
{
    public string Name { get; set; }
    public string ApiVersion { get; set; } = "v1alpha1";
    public string Group { get; set; } = "moira.operator";
}