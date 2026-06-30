using k8s.Models;
using KubeOps.Abstractions.Entities;

namespace Moira.Authentik.KubeOps.Entities;

[KubernetesEntity(Group = "moira.operator", ApiVersion = "v1alpha1", Kind = "AuthentikGroupSettings")]
public class AuthentikGroupSettings : CustomKubernetesEntity<AuthentikGroupSettings.SettingsSpec, AuthentikGroupSettings.SettingsStatus>
{
    public class SettingsSpec {}

    public class SettingsStatus
    {
        public long? ObservedGeneration { get; set; } = 0;
    }
}