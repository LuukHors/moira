using KubeOps.Abstractions.Entities;

namespace Moira.Common.Kubernetes.Secrets.Models;

public static class OidcSecretNames
{
    public static string SourceSecretName(CustomKubernetesEntity entity) => $"{entity.Metadata.Name}-oidc-credentials";
}
