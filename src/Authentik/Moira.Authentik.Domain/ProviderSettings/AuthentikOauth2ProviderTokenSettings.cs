namespace Moira.Authentik.Domain.ProviderSettings;

public record AuthentikOauth2ProviderTokenSettings(
    string AccessCodeValidity = "hours=1",
    string AccessTokenValidity = "hours=3",
    string RefreshTokenValidity = "days=2");