using KubeOps.Operator.Web.Webhooks.Admission.Validation;
using Moira.KubeOps.Entities;
using Moira.KubeOps.PreReconcileSteps.ValidatorWebhooks.Executor;

namespace Moira.KubeOps.PreReconcileSteps.ValidatorWebhooks;

[ValidationWebhook(typeof(OidcApplication))]
internal class OidcApplicationValidationWebhook(IValidatorExecutor<OidcApplication> validator) : ValidationWebhook<OidcApplication>
{
    public override Task<ValidationResult> CreateAsync(OidcApplication entity, bool dryRun, CancellationToken cancellation) 
        => validator.ExecuteAsync(entity, cancellation);
    public override Task<ValidationResult> UpdateAsync(OidcApplication oldEntity, OidcApplication entity, bool dryRun, CancellationToken cancellation) 
        => validator.ExecuteAsync(entity, cancellation);
}
