namespace Moira.Authentik.Provider;

public abstract class AbstractAuthentikProviderAdapter
{
    private const string ProviderName = "Authentik";
    public string Name { get; } = ProviderName;
}
