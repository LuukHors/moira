using k8s.Models;
using KubeOps.Abstractions.Entities;

namespace Moira.KubeOps.Entities;

[KubernetesEntity(Group = "moira.operator", ApiVersion = "v1alpha1", Kind = "AuthentikOidcApplicationSettings")]
public class AuthentikOidcApplicationSettings : CustomKubernetesEntity<AuthentikOidcApplicationSettings.SettingsSpec, AuthentikOidcApplicationSettings.SettingsStatus>
{
    public class SettingsSpec
    {
        public string AuthorizationFlowSlug { get; set; } = "default-provider-authorization-explicit-consent";
        public string InvalidationFlowSlug { get; set; } = "default-provider-invalidation-flow";
        public string RedirectUriMatchingMode { get; set; } = "strict";
        public string Group { get; set; } = string.Empty;
        public TokenSettings TokenSettings { get; set; } = new();
        public MetadataSettings MetadataSettings { get; set; } = new();
    }

    public class TokenSettings
    {
        public string AccessCodeValidity = string.Empty;
        public string AccessTokenValidity = string.Empty;
        public string RefreshTokenValidity = string.Empty;
    }

    public class MetadataSettings
    {
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public bool OpenInNewTab { get; set; } = false;
    }

    public class SettingsStatus
    {
        public long? ObservedGeneration { get; set; } = 0;
    }
}
