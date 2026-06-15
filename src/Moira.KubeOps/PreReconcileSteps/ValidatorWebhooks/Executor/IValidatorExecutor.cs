using k8s;
using k8s.Models;
using KubeOps.Operator.Web.Webhooks.Admission.Validation;

namespace Moira.KubeOps.ValidatorWebhooks.Executor;

public interface IValidatorExecutor<in TK8SEntity> where TK8SEntity : IKubernetesObject<V1ObjectMeta>
{
    Task<ValidationResult> ExecuteAsync(TK8SEntity entity, CancellationToken cancellationToken);
}