using KubeOps.Operator.Web.Webhooks.Admission.Validation;
using Moira.Authentik.Kubernetes.Role;
using Moira.Common.Kubernetes.PreReconcileSteps.ValidatorWebhooks.Executor;

namespace Moira.Authentik.Kubernetes.ValidatorWebhooks;

[ValidationWebhook(typeof(AuthentikRole))]
internal class AuthentikRoleValidationWebhook(IValidatorExecutor<AuthentikRole> validator) : ValidationWebhook<AuthentikRole>
{
    public override Task<ValidationResult> CreateAsync(AuthentikRole entity, bool dryRun, CancellationToken cancellation) => validator.ExecuteAsync(entity, cancellation);
    public override Task<ValidationResult> UpdateAsync(AuthentikRole oldEntity, AuthentikRole entity, bool dryRun, CancellationToken cancellation) => validator.ExecuteAsync(entity, cancellation);
}
