namespace Moira.KubeOps.Status;

public static class ConditionReasons
{
    public const string ReconcileSucceeded = "ReconcileSucceeded";
    public const string ReconcileFailed = "ReconcileFailed";
    public const string DependenciesResolved = "DependenciesResolved";
    public const string DependencyMissing = "DependencyMissing";
    public const string DeleteStarted = "DeleteStarted";
    public const string DeleteSucceeded = "DeleteSucceeded";
    public const string DeleteSkipped = "DeleteSkipped";
    public const string DeleteFailed = "DeleteFailed";
    public const string ProviderCheckSucceeded = "ProviderCheckSucceeded";
    public const string ProviderCheckFailed = "ProviderCheckFailed";
}
