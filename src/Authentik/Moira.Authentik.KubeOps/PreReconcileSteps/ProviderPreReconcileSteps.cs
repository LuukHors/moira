using Moira.Common.KubeOps.PreReconcileSteps;
using Moira.Authentik.KubeOps.Entities;

namespace Moira.Authentik.KubeOps.PreReconcileSteps;

public class ProviderPreReconcileSteps : IPreReconcileSteps<AuthentikProvider>
{
    public Task<bool> ExecuteAsync(AuthentikProvider entity, CancellationToken cancellationToken)
    {
        return Task.FromResult(false);
    }
}
