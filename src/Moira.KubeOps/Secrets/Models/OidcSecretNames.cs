using k8s.Models;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.Secrets.Models;

internal static class OidcSecretNames
{
    public static string SourceSecretName(OidcApplication entity) => $"{entity.Name()}-oidc-credentials";
}
