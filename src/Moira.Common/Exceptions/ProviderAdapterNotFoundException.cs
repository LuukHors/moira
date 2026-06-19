namespace Moira.Common.Exceptions;

public class ProviderAdapterNotFoundException(string providerName)
    : DependencyException($"No adapter with name \"{providerName}\" found.", MoiraExceptionReason.ProviderAdapterMissing)
{
    public string ProviderName { get; } = providerName;
}
