namespace Moira.Authentik.Domain.Applications;

public record AuthentikOidcApplicationV3(
    AuthentikApplicationV3 Application,
    AuthentikOAuth2ProviderV3? Provider);
