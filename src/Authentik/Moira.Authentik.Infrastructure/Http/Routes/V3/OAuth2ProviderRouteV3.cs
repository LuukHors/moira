using Moira.Authentik.Domain.Applications;

namespace Moira.Authentik.Infrastructure.Http.Routes.V3;

internal sealed class OAuth2ProviderRouteV3 : IAuthentikRoute<AuthentikOAuth2ProviderV3, AuthentikOAuth2ProviderV3, int>
{
    public string CollectionEntityPath => "providers/oauth2/";
    public string SingleEntityPath(int id) => $"providers/oauth2/{id}/";
}
