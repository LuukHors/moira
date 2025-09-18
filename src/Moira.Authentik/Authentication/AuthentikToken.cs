namespace Moira.Authentik.Authentication;

public record AuthentikToken(string Token, DateTime ExpiresAt);