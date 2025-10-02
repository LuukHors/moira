using Moira.Authentik.Models.V3;

namespace Moira.Authentik.HttpService.Routes;

public sealed class GroupRoute : IAuthentikRoute<AuthentikGroupV3, AuthentikGroupV3, string>
{
    public string CollectionPath => "core/groups";
    public string SinglePath(string id) => $"core/groups/{id}";
}