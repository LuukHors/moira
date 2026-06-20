namespace Moira.Authentik.Models.V3;

public record AuthentikOidcApplicationV3(
    AuthentikApplicationV3 Application,
    AuthentikOAuth2ProviderV3? Provider)
{
    public bool IsComplete => Provider is not null;
}
