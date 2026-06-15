using KubeOps.Abstractions.Finalizer;
using Moira.KubeOps.Controllers;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.PreReconcileSteps;

public class OidcApplicationPreReconcileSteps(EntityFinalizerAttacher<OidcApplicationFinalizer, OidcApplication> finalizer) : IPreReconcileSteps<OidcApplication>
{
    public Task<bool> ExecuteAsync(OidcApplication entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}