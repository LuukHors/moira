namespace Moira.Authentik.ProviderAdapters;

public abstract class AbstractAuthentikProviderAdapter
{
    private const string ProviderName = "Authentik";
    public string Name => ProviderName;
}
