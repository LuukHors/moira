using Moira.Authentik.Models.V3;

namespace Moira.Authentik.HttpService.Routes;

public sealed class OAuth2ProviderRoute : IAuthentikRoute<AuthentikOAuth2ProviderV3, AuthentikOAuth2ProviderV3, string>
{
    public string CollectionEntityPath => "providers/oauth2/";
    public string SingleEntityPath(string id) => $"providers/oauth2/{id}/";
}
