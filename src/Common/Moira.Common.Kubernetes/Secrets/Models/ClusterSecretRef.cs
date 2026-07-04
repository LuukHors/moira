namespace Moira.Common.Kubernetes.Secrets.Models;

public record ClusterSecretRef
{
    public string Name { get; init; } = string.Empty;
    public string Namespace { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
}
