using Moira.Authentik.Domain.Applications;

namespace Moira.Authentik.Infrastructure.Http.Routes.V3;

internal sealed class ApplicationRouteV3 : IAuthentikRoute<AuthentikApplicationV3, AuthentikApplicationV3, string>
{
    public string CollectionEntityPath => "core/applications/";
    public string SingleEntityPath(string id) => $"core/applications/{id}/";
}
