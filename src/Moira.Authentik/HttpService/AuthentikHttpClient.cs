using Moira.Authentik.Models.V3;

namespace Moira.Authentik.HttpService;

public class AuthentikHttpClient(
    IHttpService<AuthentikGroupV3, AuthentikGroupV3> groupsHandler) : IAuthentikHttpClient
{
    public IHttpService<AuthentikGroupV3, AuthentikGroupV3> Groups { get; } = groupsHandler;
}