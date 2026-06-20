namespace Moira.Authentik.Models.V3;

public record AuthentikRedirectUriV3(
    string matching_mode,
    string url,
    string redirect_uri_type = "authorization");
