using KubeOps.Abstractions.Finalizer;
using Microsoft.Extensions.Logging;
using Moira.Common.Abstractions.Exceptions;
using Moira.KubeOps.Controllers;
using Moira.KubeOps.Entities;
using Moira.KubeOps.PreReconcileSteps.ValidatorWebhooks.Executor;

namespace Moira.KubeOps.PreReconcileSteps;

public class OidcApplicationPreReconcileSteps(
    EntityFinalizerAttacher<OidcApplicationFinalizer, OidcApplication> finalizer,
    IValidatorExecutor<OidcApplication> validator,
    ILogger<OidcApplicationPreReconcileSteps> logger) : IPreReconcileSteps<OidcApplication>
{
    public async Task<bool> ExecuteAsync(OidcApplication entity, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ExecuteAsync(entity, cancellationToken);
        if (!validationResult.Valid)
        {
            var message = validationResult.Status?.Message ?? "OIDC application validation failed.";
            throw new EntityValidationException(message);
        }

        var result = await finalizer(entity, cancellationToken);
        var finalizerAttached = !result.Equals(entity);
        if (finalizerAttached)
        {
            logger.LogInformation("Attached OIDC application finalizer; reconciliation will continue after Kubernetes stores the update");
        }

        return finalizerAttached;
    }
}
