using Moira.Authentik.Models.V3;

namespace Moira.Authentik.HttpService;

public interface IAuthentikHttpClient
{
    IHttpService<AuthentikGroupV3, AuthentikGroupV3> Groups { get; }
}