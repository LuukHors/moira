namespace Moira.Common.KubeOps.Status;

public static class ConditionTypes
{
    public const string Ready = "Ready";
    public const string DependenciesReady = "DependenciesReady";
    public const string SecretsReady = "SecretsReady";
    public const string RotationReady = "RotationReady";
    public const string Deleting = "Deleting";
}
