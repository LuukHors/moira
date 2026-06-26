using Moira.Authentik.Domain.Applications;

namespace Moira.Authentik.Infrastructure.Http.Routes.V3;

internal sealed class FlowRouteV3 : IAuthentikRoute<AuthentikFlowV3, AuthentikFlowV3, string>
{
    public string CollectionEntityPath => "flows/instances/";
    public string SingleEntityPath(string id) => $"flows/instances/{id}/";
}
