using Moira.Authentik.Models.V3;

namespace Moira.Authentik.HttpService.Routes;

public sealed class ScopeMappingRoute : IAuthentikRoute<AuthentikScopeMappingV3, AuthentikScopeMappingV3, string>
{
    public string CollectionEntityPath => "propertymappings/provider/scope/";
    public string SingleEntityPath(string id) => $"propertymappings/provider/scope/{id}/";
}
