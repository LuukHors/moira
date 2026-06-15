using KubeOps.Operator.Web.Webhooks.Admission.Validation;
using Moira.KubeOps.Entities;
using Moira.KubeOps.ValidatorWebhooks.Executor;

namespace Moira.KubeOps.ValidatorWebhooks;

[ValidationWebhook(typeof(Provider))]
public class ProviderValidationWebhook(IValidatorExecutor<Provider> validator) : ValidationWebhook<Provider>
{
    public override Task<ValidationResult> CreateAsync(Provider entity, bool dryRun, CancellationToken cancellation) => validator.ExecuteAsync(entity, cancellation);

    public override Task<ValidationResult> UpdateAsync(Provider oldEntity, Provider entity, bool dryRun, CancellationToken cancellation) => validator.ExecuteAsync(entity, cancellation);
}