namespace Moira.Authentik.Domain.Applications;

public record AuthentikRedirectUriV3(
    string matching_mode,
    string url,
    string redirect_uri_type = "authorization");
