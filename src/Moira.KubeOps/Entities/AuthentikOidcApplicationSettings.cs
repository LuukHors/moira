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
    }

    public class SettingsStatus
    {
        public long? ObservedGeneration { get; set; } = 0;
    }
}
