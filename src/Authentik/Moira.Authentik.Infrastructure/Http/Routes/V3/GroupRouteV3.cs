using Moira.Authentik.Domain.Groups;

namespace Moira.Authentik.Infrastructure.Http.Routes.V3;

internal sealed class GroupRouteV3 : IAuthentikRoute<AuthentikGroupV3, AuthentikGroupV3, string>
{
    public string CollectionEntityPath => "core/groups/";
    public string SingleEntityPath(string id) => $"core/groups/{id}/";
}
