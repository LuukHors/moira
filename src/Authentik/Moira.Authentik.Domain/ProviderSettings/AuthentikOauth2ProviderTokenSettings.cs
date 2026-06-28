namespace Moira.Authentik.Domain.ProviderSettings;

public record AuthentikOauth2ProviderTokenSettings(
    string? AccessCodeValidity = null,
    string? AccessTokenValidity = null,
    string? RefreshTokenValidity = null);