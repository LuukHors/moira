using Moira.Authentik.Models.V3;

namespace Moira.Authentik.HttpService.Routes;

public sealed class FlowRoute : IAuthentikRoute<AuthentikFlowV3, AuthentikFlowV3, string>
{
    public string CollectionEntityPath => "flows/instances/";
    public string SingleEntityPath(string id) => $"flows/instances/{id}/";
}
