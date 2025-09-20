using FluentValidation;
using k8s;
using k8s.Models;
using KubeOps.Operator.Web.Webhooks.Admission.Validation;
using Moira.KubeOps.ValidatorWebhooks.Utilities;

namespace Moira.KubeOps.ValidatorWebhooks.Executor;

public class ValidatorExecutor<TK8SEntity>(AbstractValidator<TK8SEntity> validator) : ValidationWebhook<TK8SEntity>, IValidatorExecutor<TK8SEntity> where TK8SEntity : IKubernetesObject<V1ObjectMeta>
{
    public async Task<ValidationResult> ExecuteAsync(TK8SEntity entity, CancellationToken cancellationToken)
    {
        try
        {
            await validator.ValidateAndThrowAsync(entity, cancellationToken);
            return Success();
        }
        catch (ValidationException ex)
        {
            var errorMessage = ex.FormatError();
            return Fail(errorMessage);
        }
    }
}
