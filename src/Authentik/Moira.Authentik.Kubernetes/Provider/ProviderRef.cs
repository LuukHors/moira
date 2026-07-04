namespace Moira.Authentik.Kubernetes.Provider;

public class ProviderRef
{
    public string Name { get; set; }
    public string Namespace { get; set; } = "default";
    public string ApiVersion { get; set; } = "v1alpha1";
    public string AuthentikGroup { get; set; } = "moira.operator";
}