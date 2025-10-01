using Moira.Authentik.Authentication;
using Moira.Authentik.Models.V3;

namespace Moira.Authentik.HttpService;

public class AuthentikGroupHttpService2(IAuthentikAuthenticationService authService) : AbstractAuthentikHttpService<AuthentikGroupV3, AuthentikGroupV3>(authService)
{
    protected override string BasePath => "/core/groups/";
    protected override string BasePathWithIdentifier => "/core/groups/{id}/";
}