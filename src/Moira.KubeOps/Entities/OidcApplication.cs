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
        public ResourceRef? ProviderSettingsRef { get; set; }
        public bool AutoDelete { get; set; } = true;
        public OidcSpec Oidc { get; set; } = new();
        public IEnumerable<Secret> Secrets { get; set; } = [];
        public int RotationDays { get; set; } = 90;
    }

    public class ApplicationStatus
    {
        public string ApplicationId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public IReadOnlyDictionary<string, string> ProviderResourceIds { get; set; } = new Dictionary<string, string>();
        public long? ObservedGeneration { get; set; } = 0;
        public DateTime? LastRotatedAt { get; set; }
        public DateTime? NextRotationAt { get; set; }
        public IEnumerable<SecretStatus> Secrets { get; set; } = [];
        public IList<V1Condition> Conditions { get; set; } = new List<V1Condition>();
    }

    public class Secret
    {
        public string Name { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
        public ClusterSecretRef? ClusterSecretRef { get; set; }
        public string Type { get; set; } = "Opaque";
        public IDictionary<string, string> Labels { get; set; } = new Dictionary<string, string>();
        public IDictionary<string, string> Annotations { get; set; } = new Dictionary<string, string>();
        public IDictionary<string, string> Template { get; set; } = new Dictionary<string, string>();
    }

    public class ClusterSecretRef
    {
        public string Name { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
    }

    public class SecretStatus
    {
        public string Name { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
        public string Cluster { get; set; } = "local";
        public ClusterSecretRef? ClusterRef { get; set; }
        public DateTime? LastSyncedAt { get; set; }
        public bool Synced { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
