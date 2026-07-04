using KubeOps.Abstractions.Reconciliation.Finalizer;
using Microsoft.Extensions.Logging;
using Moira.Authentik.Kubernetes.Controllers;
using Moira.Common.Abstractions.Exceptions;
using Moira.Common.Kubernetes.PreReconcileSteps;
using Moira.Common.Kubernetes.PreReconcileSteps.ValidatorWebhooks.Executor;

namespace Moira.Authentik.Kubernetes.OidcApplication;

public class AuthentikOidcApplicationPreReconcileSteps(
    EntityFinalizerAttacher<OidcApplicationFinalizer, AuthentikOidcApplication> finalizer,
    IValidatorExecutor<AuthentikOidcApplication> validator,
    ILogger<AuthentikOidcApplicationPreReconcileSteps> logger) : IPreReconcileSteps<AuthentikOidcApplication>
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
