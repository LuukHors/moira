using k8s.Models;
using KubeOps.Abstractions.Entities;

namespace Moira.KubeOps.Entities;

[KubernetesEntity(Group = "moira.operator", ApiVersion = "v1alpha1", Kind = "OidcApplication")]
public class OidcApplication : CustomKubernetesEntity<OidcApplication.ApplicationSpec, OidcApplication.ApplicationStatus>
{
    public class ApplicationSpec
    {
        public string DisplayName { get; set; } = string.Empty;
        public ProviderRef ProviderRef { get; set; } = new();
        public bool AutoDelete { get; set; } = true;
        public OidcSpec Oidc { get; set; } = new();
        public IEnumerable<SecretTarget> SecretTargets { get; set; } = [];
        public int RotationDays { get; set; } = 90;
    }

    public class ApplicationStatus
    {
        public string ApplicationId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public long? ObservedGeneration { get; set; } = 0;
        public DateTime? LastRotatedAt { get; set; }
        public DateTime? NextRotationAt { get; set; }
        public IEnumerable<SecretTargetStatus> SecretTargets { get; set; } = [];
        public IList<V1Condition> Conditions { get; set; } = new List<V1Condition>();
    }

    public class SecretTarget
    {
        public string Name { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
        public ClusterRef? ClusterRef { get; set; }
        public string Type { get; set; } = "Opaque";
        public IDictionary<string, string> Labels { get; set; } = new Dictionary<string, string>();
        public IDictionary<string, string> Annotations { get; set; } = new Dictionary<string, string>();
        public SecretTargetKeys Keys { get; set; } = new();
    }

    public class ClusterRef
    {
        public KubeConfigSecretRef KubeConfigSecretRef { get; set; } = new();
    }

    public class KubeConfigSecretRef
    {
        public string Name { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
    }

    public class SecretTargetKeys
    {
        public string ClientId { get; set; } = "ClientId";
        public string ClientSecret { get; set; } = "ClientSecret";
    }

    public class SecretTargetStatus
    {
        public string Name { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
        public string Cluster { get; set; } = "local";
        public DateTime? LastSyncedAt { get; set; }
        public bool Synced { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
