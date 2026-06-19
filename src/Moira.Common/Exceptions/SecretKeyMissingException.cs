namespace Moira.Common.Exceptions;

public class SecretKeyMissingException(string secretNamespace, string secretName, IEnumerable<string> missingKeys)
    : DependencyException(
        $"Could not get key/value {string.Join(" or ", missingKeys)} from secret {secretNamespace}/{secretName}",
        MoiraExceptionReason.SecretKeyMissing)
{
    public string SecretNamespace { get; } = secretNamespace;
    public string SecretName { get; } = secretName;
    public IReadOnlyCollection<string> MissingKeys { get; } = missingKeys.ToArray();
}
