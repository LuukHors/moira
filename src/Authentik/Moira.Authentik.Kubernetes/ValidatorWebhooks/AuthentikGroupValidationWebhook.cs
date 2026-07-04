using KubeOps.Operator.Web.Webhooks.Admission.Validation;
using Moira.Authentik.Kubernetes.Group;
using Moira.Common.Kubernetes.PreReconcileSteps.ValidatorWebhooks.Executor;

namespace Moira.Authentik.Kubernetes.ValidatorWebhooks;

[ValidationWebhook(typeof(AuthentikGroup))]
internal class AuthentikGroupValidationWebhook(IValidatorExecutor<AuthentikGroup> validator) : ValidationWebhook<AuthentikGroup>
{
    public override Task<ValidationResult> CreateAsync(AuthentikGroup entity, bool dryRun, CancellationToken cancellation) => validator.ExecuteAsync(entity, cancellation);
    public override Task<ValidationResult> UpdateAsync(AuthentikGroup oldEntity, AuthentikGroup entity, bool dryRun, CancellationToken cancellation) => validator.ExecuteAsync(entity, cancellation);
}
