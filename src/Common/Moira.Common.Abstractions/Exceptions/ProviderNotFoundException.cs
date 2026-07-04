namespace Moira.Common.Abstractions.Exceptions;

public class ProviderNotFoundException(string providerNamespace, string providerName)
    : DependencyException($"Unable to get provider with name \"{providerName}\" in namespace \"{providerNamespace}\"", MoiraExceptionReason.ProviderMissing)
{
    public string ProviderNamespace { get; } = providerNamespace;
    public string ProviderName { get; } = providerName;
}
