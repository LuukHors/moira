namespace Moira.Common.KubeOps.Status;

public static class ConditionReasons
{
    public const string ReconcileSucceeded = "ReconcileSucceeded";
    public const string ReconcileFailed = "ReconcileFailed";
    public const string DependenciesResolved = "DependenciesResolved";
    public const string DependencyMissing = "DependencyMissing";
    public const string SecretMissing = "SecretMissing";
    public const string SecretKeyMissing = "SecretKeyMissing";
    public const string ProviderMissing = "ProviderMissing";
    public const string ProviderAdapterMissing = "ProviderAdapterMissing";
    public const string ValidationFailed = "ValidationFailed";
    public const string IdpRequestFailed = "IdpRequestFailed";
    public const string IdpValidationFailed = "IdpValidationFailed";
    public const string DeleteStarted = "DeleteStarted";
    public const string DeleteSucceeded = "DeleteSucceeded";
    public const string DeleteSkipped = "DeleteSkipped";
    public const string DeleteFailed = "DeleteFailed";
    public const string ProviderCheckSucceeded = "ProviderCheckSucceeded";
    public const string ProviderCheckFailed = "ProviderCheckFailed";
    public const string SecretSyncSucceeded = "SecretSyncSucceeded";
    public const string SecretSyncFailed = "SecretSyncFailed";
    public const string RotationNotDue = "RotationNotDue";
    public const string RotationSucceeded = "RotationSucceeded";
    public const string RotationFailed = "RotationFailed";
}
