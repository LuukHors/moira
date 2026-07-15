using KubeOps.KubernetesClient;
using Moira.Common.Kubernetes.PreReconcileSteps;

namespace Moira.Authentik.Kubernetes.Provider;

public class ProviderPreReconcileSteps(IKubernetesClient client) : IPreReconcileSteps<AuthentikProvider>
{
    public Task<bool> ExecuteAsync(AuthentikProvider entity, CancellationToken cancellationToken)
    {
        return Task.FromResult(false);
    }
}
