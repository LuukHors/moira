using k8s.Models;
using KubeOps.Abstractions.Entities;

namespace Moira.Authentik.KubeOps.Entities;

[KubernetesEntity(Group = "moira.operator", ApiVersion = "v1alpha1", Kind = "AuthentikOidcApplication")]
public class AuthentikOidcApplication : CustomKubernetesEntity<AuthentikOidcApplication.ApplicationSpec, AuthentikOidcApplication.ApplicationStatus>
{
    public class ApplicationSpec
    {
        public string DisplayName { get; set; } = string.Empty;
        public ProviderRef ProviderRef { get; set; } = new();
        public bool AutoDelete { get; set; } = true;
        public OidcSpec Oidc { get; set; } = new();
        public IEnumerable<Secret> Secrets { get; set; } = [];
        public int RotationDays { get; set; } = 90;

        // Authentik-specific settings (formerly the AuthentikOidcApplicationSettings CRD)
        public string AuthorizationFlowSlug { get; set; } = "default-provider-authorization-explicit-consent";
        public string InvalidationFlowSlug { get; set; } = "default-provider-invalidation-flow";
        public string RedirectUriMatchingMode { get; set; } = "strict";
        public string Group { get; set; } = string.Empty;
        public TokenSettings TokenSettings { get; set; } = new();
        public MetadataSettings MetadataSettings { get; set; } = new();
    }

    public class TokenSettings
    {
        public string AccessCodeValidity { get; set; } = string.Empty;
        public string AccessTokenValidity { get; set; } = string.Empty;
        public string RefreshTokenValidity { get; set; } = string.Empty;
    }

    public class MetadataSettings
    {
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public bool OpenInNewTab { get; set; } = false;
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