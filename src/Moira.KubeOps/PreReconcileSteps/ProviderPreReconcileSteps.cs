using Moira.KubeOps.Entities;

namespace Moira.KubeOps.PreReconcileSteps;

public class ProviderPreReconcileSteps : IPreReconcileSteps<Provider>
{
    public Task<bool> ExecuteAsync(Provider entity, CancellationToken cancellationToken)
    {
        return Task.FromResult(false);
    }
}
