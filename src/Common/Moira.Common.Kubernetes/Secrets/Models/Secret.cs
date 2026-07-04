namespace Moira.Common.Kubernetes.Secrets.Models;

public record Secret
{
    public string Name { get; init; } = string.Empty;
    public string Namespace { get; init; } = string.Empty;
    public ClusterSecretRef? ClusterRef { get; init; }
    public string Type { get; init; } = "Opaque";
    public IDictionary<string, string> Labels { get; init; } = new Dictionary<string, string>();
    public IDictionary<string, string> Annotations { get; init; } = new Dictionary<string, string>();
    public IDictionary<string, string> Data { get; init; } = new Dictionary<string, string>();
    public IDictionary<string, string> Template { get; } = new Dictionary<string, string>();
}
