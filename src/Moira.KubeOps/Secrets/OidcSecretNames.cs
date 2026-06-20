using k8s.Models;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.Secrets;

internal static class OidcSecretNames
{
    public static string SourceSecretName(OidcApplication entity) => $"{entity.Name()}-oidc-credentials";
}
