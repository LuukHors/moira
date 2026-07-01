using KubeOps.Operator.Web.Webhooks.Admission.Validation;
using Moira.Authentik.KubeOps.Entities;
using Moira.Common.KubeOps.PreReconcileSteps.ValidatorWebhooks.Executor;

namespace Moira.Authentik.KubeOps.PreReconcileSteps.ValidatorWebhooks;

[ValidationWebhook(typeof(AuthentikOidcApplication))]
internal class OidcApplicationValidationWebhook(IValidatorExecutor<AuthentikOidcApplication> validator) : ValidationWebhook<AuthentikOidcApplication>
{
    public override Task<ValidationResult> CreateAsync(AuthentikOidcApplication entity, bool dryRun, CancellationToken cancellation) 
        => validator.ExecuteAsync(entity, cancellation);
    public override Task<ValidationResult> UpdateAsync(AuthentikOidcApplication oldEntity, AuthentikOidcApplication entity, bool dryRun, CancellationToken cancellation) 
        => validator.ExecuteAsync(entity, cancellation);
}
