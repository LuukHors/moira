using KubeOps.Operator.Web.Webhooks.Admission.Validation;
using Moira.KubeOps.Entities;
using Moira.KubeOps.ValidatorWebhooks.Executor;

namespace Moira.KubeOps.ValidatorWebhooks;

[ValidationWebhook(typeof(Group))]
public class GroupValidationWebhook(IValidatorExecutor<Group> validator) : ValidationWebhook<Group>
{
    public override Task<ValidationResult> CreateAsync(Group entity, bool dryRun, CancellationToken cancellation) => validator.ExecuteAsync(entity, cancellation);

    public override Task<ValidationResult> UpdateAsync(Group oldEntity, Group entity, bool dryRun, CancellationToken cancellation) => validator.ExecuteAsync(entity, cancellation);
}