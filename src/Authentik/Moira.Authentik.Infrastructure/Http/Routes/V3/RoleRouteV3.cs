using Moira.Authentik.Domain.Roles;

namespace Moira.Authentik.Infrastructure.Http.Routes.V3;

public class RoleRouteV3 : IAuthentikRoute<AuthentikRoleV3, AuthentikRoleV3, string>
{
    public string CollectionEntityPath => "rbac/roles/";
    public string SingleEntityPath(string id) => $"rbac/roles/{id}/";
}