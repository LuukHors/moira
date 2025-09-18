namespace Moira.Authentik.Authentication;

public record AuthentikAuthenticationResponseBody(string access_token, int expires_in, string token_type, string scope);