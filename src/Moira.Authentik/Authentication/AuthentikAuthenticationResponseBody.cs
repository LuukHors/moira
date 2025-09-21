namespace Moira.Authentik.Authentication;

public record AuthentikAuthenticationResponseBody(string access_token, long expires_in, string token_type, string scope);