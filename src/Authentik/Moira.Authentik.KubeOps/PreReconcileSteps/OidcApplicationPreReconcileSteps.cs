using Moira.Common.KubeOps.PreReconcileSteps;
using KubeOps.Abstractions.Finalizer;
using Microsoft.Extensions.Logging;
using Moira.Common.Abstractions.Exceptions;
using Moira.Authentik.KubeOps.Controllers;
using Moira.Authentik.KubeOps.Entities;
using Moira.Common.KubeOps.PreReconcileSteps.ValidatorWebhooks.Executor;

namespace Moira.Authentik.KubeOps.PreReconcileSteps;

public class OidcApplicationPreReconcileSteps(
    EntityFinalizerAttacher<OidcApplicationFinalizer, AuthentikOidcApplication> finalizer,
    IValidatorExecutor<AuthentikOidcApplication> validator,
    ILogger<OidcApplicationPreReconcileSteps> logger) : IPreReconcileSteps<AuthentikOidcApplication>
{
    public async Task<bool> ExecuteAsync(AuthentikOidcApplication entity, CancellationToken cancellationToken)
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
