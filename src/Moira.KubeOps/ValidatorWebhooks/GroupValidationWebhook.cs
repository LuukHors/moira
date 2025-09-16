using KubeOps.Operator.Web.Webhooks.Admission.Validation;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.ValidatorWebhooks;

[ValidationWebhook(typeof(Group))]
public class GroupValidationWebhook : ValidationWebhook<Group>
{
    public override ValidationResult Create(Group entity, bool dryRun)
    {
        return Fail("Test webhook");
    }
    
}