using KubeOps.Operator.Web.Webhooks.Admission.Validation;
using Moira.Authentik.Kubernetes.Provider;
using Moira.Common.Kubernetes.PreReconcileSteps.ValidatorWebhooks.Executor;

namespace Moira.Authentik.Kubernetes.ValidatorWebhooks;

[ValidationWebhook(typeof(AuthentikProvider))]
public class AuthentikProviderValidationWebhook(IValidatorExecutor<AuthentikProvider> validator) : ValidationWebhook<AuthentikProvider>
{
    public override Task<ValidationResult> CreateAsync(AuthentikProvider entity, bool dryRun, CancellationToken cancellation) => validator.ExecuteAsync(entity, cancellation);

    public override Task<ValidationResult> UpdateAsync(AuthentikProvider oldEntity, AuthentikProvider entity, bool dryRun, CancellationToken cancellation) => validator.ExecuteAsync(entity, cancellation);
}
