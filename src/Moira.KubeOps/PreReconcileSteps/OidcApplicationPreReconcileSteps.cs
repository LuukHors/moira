using KubeOps.Abstractions.Finalizer;
using Microsoft.Extensions.Logging;
using Moira.KubeOps.Controllers;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.PreReconcileSteps;

public class OidcApplicationPreReconcileSteps(
    EntityFinalizerAttacher<OidcApplicationFinalizer, OidcApplication> finalizer,
    ILogger<OidcApplicationPreReconcileSteps> logger) : IPreReconcileSteps<OidcApplication>
{
    public async Task<bool> ExecuteAsync(OidcApplication entity, CancellationToken cancellationToken)
    {
        var result = await finalizer(entity, cancellationToken);
        var finalizerAttached = !result.Equals(entity);
        if (finalizerAttached)
        {
            logger.LogInformation("Attached OIDC application finalizer; reconciliation will continue after Kubernetes stores the update");
        }

        return finalizerAttached;
    }
}
