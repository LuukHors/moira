namespace Moira.KubeOps.Status;

public static class ConditionTypes
{
    public const string Ready = "Ready";
    public const string DependenciesReady = "DependenciesReady";
    public const string Deleting = "Deleting";
}
