using KubeOps.Operator.Web.Webhooks.Admission.Validation;
using Moira.KubeOps.Entities;
using Moira.KubeOps.PreReconcileSteps.ValidatorWebhooks.Executor;

namespace Moira.KubeOps.PreReconcileSteps.ValidatorWebhooks;

[ValidationWebhook(typeof(AuthentikOidcApplicationSettings))]
internal class AuthentikOidcApplicationSettingsValidationWebhook(IValidatorExecutor<AuthentikOidcApplicationSettings> validator) : ValidationWebhook<AuthentikOidcApplicationSettings>
{
    public override Task<ValidationResult> CreateAsync(AuthentikOidcApplicationSettings entity, bool dryRun, CancellationToken cancellation)
        => validator.ExecuteAsync(entity, cancellation);

    public override Task<ValidationResult> UpdateAsync(AuthentikOidcApplicationSettings oldEntity, AuthentikOidcApplicationSettings entity, bool dryRun, CancellationToken cancellation)
        => validator.ExecuteAsync(entity, cancellation);
}
