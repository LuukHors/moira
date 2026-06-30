using Moira.Common.Abstractions.Exceptions;
using Moira.KubeOps.Status;

namespace Moira.KubeOps.Mappers;

public static class MoiraExceptionConditionReasonMapper
{
    public static string ToReconcileFailureReason(this MoiraException exception)
    {
        return exception.Reason switch
        {
            MoiraExceptionReason.ValidationFailed => ConditionReasons.ValidationFailed,
            MoiraExceptionReason.IdpRequestFailed => ConditionReasons.IdpRequestFailed,
            MoiraExceptionReason.IdpValidationFailed => ConditionReasons.IdpValidationFailed,
            _ => ConditionReasons.ReconcileFailed
        };
    }

    public static string ToProviderCheckFailureReason(this MoiraException exception)
    {
        return exception.Reason switch
        {
            MoiraExceptionReason.IdpRequestFailed => ConditionReasons.IdpRequestFailed,
            MoiraExceptionReason.IdpValidationFailed => ConditionReasons.IdpValidationFailed,
            _ => ConditionReasons.ProviderCheckFailed
        };
    }

    public static string ToDependencyFailureReason(this MoiraException exception)
    {
        return exception.Reason switch
        {
            MoiraExceptionReason.SecretMissing => ConditionReasons.SecretMissing,
            MoiraExceptionReason.SecretKeyMissing => ConditionReasons.SecretKeyMissing,
            MoiraExceptionReason.ProviderMissing => ConditionReasons.ProviderMissing,
            MoiraExceptionReason.ProviderAdapterMissing => ConditionReasons.ProviderAdapterMissing,
            _ => ConditionReasons.DependencyMissing
        };
    }
}
