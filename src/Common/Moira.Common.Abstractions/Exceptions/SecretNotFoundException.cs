namespace Moira.Common.Abstractions.Exceptions;

public class SecretNotFoundException(string secretNamespace, string secretName)
    : DependencyException($"Could not get secret {secretNamespace}/{secretName}", MoiraExceptionReason.SecretMissing)
{
    public string SecretNamespace { get; } = secretNamespace;
    public string SecretName { get; } = secretName;
}
