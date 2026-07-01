namespace Moira.Common.KubeOps.Secrets.Models;

public class SecretTargetStatus
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Cluster { get; set; } = "local";
    public ClusterSecretRef? ClusterRef { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public bool Synced { get; set; }
    public string Message { get; set; } = string.Empty;
}
