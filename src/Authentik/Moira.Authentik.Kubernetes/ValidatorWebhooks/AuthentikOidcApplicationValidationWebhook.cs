using KubeOps.Operator.Web.Webhooks.Admission.Validation;
using Moira.Authentik.Kubernetes.OidcApplication;
using Moira.Common.Kubernetes.PreReconcileSteps.ValidatorWebhooks.Executor;

namespace Moira.Authentik.Kubernetes.ValidatorWebhooks;

[ValidationWebhook(typeof(AuthentikOidcApplication))]
internal class AuthentikOidcApplicationValidationWebhook(IValidatorExecutor<AuthentikOidcApplication> validator) : ValidationWebhook<AuthentikOidcApplication>
{
    public override Task<ValidationResult> CreateAsync(AuthentikOidcApplication entity, bool dryRun, CancellationToken cancellation) 
        => validator.ExecuteAsync(entity, cancellation);
    public override Task<ValidationResult> UpdateAsync(AuthentikOidcApplication oldEntity, AuthentikOidcApplication entity, bool dryRun, CancellationToken cancellation) 
        => validator.ExecuteAsync(entity, cancellation);
}
