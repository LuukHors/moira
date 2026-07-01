using KubeOps.Operator.Web.Webhooks.Admission.Validation;
using Moira.Authentik.KubeOps.Entities;
using Moira.Common.KubeOps.PreReconcileSteps.ValidatorWebhooks.Executor;

namespace Moira.Authentik.KubeOps.PreReconcileSteps.ValidatorWebhooks;

[ValidationWebhook(typeof(AuthentikProvider))]
public class ProviderValidationWebhook(IValidatorExecutor<AuthentikProvider> validator) : ValidationWebhook<AuthentikProvider>
{
    public override Task<ValidationResult> CreateAsync(AuthentikProvider entity, bool dryRun, CancellationToken cancellation) => validator.ExecuteAsync(entity, cancellation);

    public override Task<ValidationResult> UpdateAsync(AuthentikProvider oldEntity, AuthentikProvider entity, bool dryRun, CancellationToken cancellation) => validator.ExecuteAsync(entity, cancellation);
}
