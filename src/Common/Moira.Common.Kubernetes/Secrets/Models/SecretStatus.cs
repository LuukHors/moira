namespace Moira.Common.Kubernetes.Secrets.Models;

public record SecretStatus
{
    public string Name { get; init; } = string.Empty;
    public string Namespace { get; init; } = string.Empty;
    public string Cluster { get; init; } = "local";
    public ClusterSecretRef? ClusterRef { get; init; }
    public DateTime? LastSyncedAt { get; set; }
    public bool Synced { get; set; }
    public string Message { get; set; } = string.Empty;
}