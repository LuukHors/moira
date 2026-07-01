using k8s.Models;
using Moira.Authentik.KubeOps.Entities;

namespace Moira.Authentik.KubeOps.Secrets.Models;

internal static class OidcSecretNames
{
    public static string SourceSecretName(AuthentikOidcApplication entity) => $"{entity.Name()}-oidc-credentials";
}
