namespace Moira.Authentik.Controllers.Adapters;

public abstract class AbstractAuthentikProviderAdapter
{
    private const string ProviderName = "Authentik";
    public string Name => ProviderName;
}
