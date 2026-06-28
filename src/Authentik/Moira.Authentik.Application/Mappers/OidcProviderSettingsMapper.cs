using Moira.Authentik.Domain.ProviderSettings;
using Moira.Common.Models;

namespace Moira.Authentik.Application.Mappers;

public static class OidcProviderSettingsMapper
{
    public static OidcAuthentikProviderSettings ToAuthentikSettings(this OidcProviderSettings providerSettings)
    {
        var openInNewTab = bool.TryParse(providerSettings.Values.GetValueOrDefault("openInNewTab"), out var parsed) && parsed;

        return new OidcAuthentikProviderSettings(
            AuthorizationFlowSlug: Get("authorizationFlowSlug"),
            InvalidationFlowSlug: Get("invalidationFlowSlug"),
            RedirectUriMatchingMode: Get("redirectUriMatchingMode"),
            Group: Get("group"))
        {
            Metadata = new AuthentikApplicationMetadataSettings(
                Description: Get("description"),
                Icon: Get("icon"),
                Publisher: Get("publisher"),
                OpenInNewTab: openInNewTab),
            TokenSettings = new AuthentikOauth2ProviderTokenSettings(
                AccessCodeValidity: Get("accessCodeValidity"),
                AccessTokenValidity: Get("accessTokenValidity"),
                RefreshTokenValidity: Get("refreshTokenValidity"))
        };

        string? Get(string key)
        {
            var value = providerSettings.Values.GetValueOrDefault(key);
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }
}