using KubeOps.Operator.Web.Webhooks.Admission.Validation;
using Moira.Authentik.KubeOps.Entities;
using Moira.Common.KubeOps.PreReconcileSteps.ValidatorWebhooks.Executor;

namespace Moira.Authentik.KubeOps.PreReconcileSteps.ValidatorWebhooks;

[ValidationWebhook(typeof(AuthentikGroup))]
internal class GroupValidationWebhook(IValidatorExecutor<AuthentikGroup> validator) : ValidationWebhook<AuthentikGroup>
{
    public override Task<ValidationResult> CreateAsync(AuthentikGroup entity, bool dryRun, CancellationToken cancellation) => validator.ExecuteAsync(entity, cancellation);
    public override Task<ValidationResult> UpdateAsync(AuthentikGroup oldEntity, AuthentikGroup entity, bool dryRun, CancellationToken cancellation) => validator.ExecuteAsync(entity, cancellation);
}
