using Moira.Authentik.Domain.Applications;

namespace Moira.Authentik.Infrastructure.Http.Routes.V3;

internal sealed class ScopeMappingRouteV3 : IAuthentikRoute<AuthentikScopeMappingV3, AuthentikScopeMappingV3, string>
{
    public string CollectionEntityPath => "propertymappings/provider/scope/";
    public string SingleEntityPath(string id) => $"propertymappings/provider/scope/{id}/";
}
