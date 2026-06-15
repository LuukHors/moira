using k8s.Models;
using KubeOps.Abstractions.Entities;

namespace Moira.KubeOps.Entities;

[KubernetesEntity(Group = "moira.operator", ApiVersion = "v1alpha1", Kind = "OidcApplication")]
public class OidcApplication : CustomKubernetesEntity<OidcApplication.ApplicationSpec, OidcApplication.ApplicationResult>
{
    public class ApplicationSpec
    {
        public string DisplayName { get; set; } = string.Empty;
        
        public OidcSpec? Oidc { get; set; }
        public SamlSpec? Saml { get; set; }
    }

    public class ApplicationResult
    {
        
    }
}