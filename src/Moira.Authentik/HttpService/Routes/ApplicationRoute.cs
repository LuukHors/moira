using Moira.Authentik.Models.V3;

namespace Moira.Authentik.HttpService.Routes;

public sealed class ApplicationRoute : IAuthentikRoute<AuthentikApplicationV3, AuthentikApplicationV3, string>
{
    public string CollectionEntityPath => "core/applications/";
    public string SingleEntityPath(string id) => $"core/applications/{id}/";
}
