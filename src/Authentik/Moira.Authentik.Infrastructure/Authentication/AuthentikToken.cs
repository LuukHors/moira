namespace Moira.Authentik.Infrastructure.Authentication;

public record AuthentikToken(string Token, DateTime ExpiresAt);
