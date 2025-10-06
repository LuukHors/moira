using Moira.Authentik.Models.V3;

namespace Moira.Authentik.HttpService;

public class AuthentikHttpClient(
    IHttpService<AuthentikGroupV3, AuthentikGroupV3, string> groupsHandler) : IAuthentikHttpClient
{
    public IHttpService<AuthentikGroupV3, AuthentikGroupV3, string> Groups { get; } = groupsHandler;
}