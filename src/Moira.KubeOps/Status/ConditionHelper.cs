using k8s.Models;
using KubeOps.Abstractions.Entities;

namespace Moira.KubeOps.Status;

public static class ConditionHelper
{
    public static void UpsertCondition(
        this CustomKubernetesEntity entity,
        IList<V1Condition> conditions,
        string type,
        string status,
        string reason,
        string message)
    {
        var existing = conditions.FirstOrDefault(condition => condition.Type == type);
        var observedGeneration = entity.Metadata.Generation;

        if (existing is null)
        {
            conditions.Add(new V1Condition
            {
                Type = type,
                Status = status,
                ObservedGeneration = observedGeneration,
                LastTransitionTime = DateTime.UtcNow,
                Reason = reason,
                Message = message
            });
            return;
        }

        var hasTransitioned = existing.Status != status
                              || existing.Reason != reason
                              || existing.Message != message;

        existing.Status = status;
        existing.ObservedGeneration = observedGeneration;
        existing.Reason = reason;
        existing.Message = message;

        if (hasTransitioned)
        {
            existing.LastTransitionTime = DateTime.UtcNow;
        }
    }
}
