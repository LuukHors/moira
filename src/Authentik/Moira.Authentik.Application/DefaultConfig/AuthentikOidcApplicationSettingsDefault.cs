using Moira.Authentik.Domain.ProviderSettings;
using Moira.Common.Provider;

namespace Moira.Authentik.Application.DefaultConfig;

public class AuthentikOidcApplicationSettingsDefault : IDefaultConfig<OidcAuthentikProviderSettings>
{
    public OidcAuthentikProviderSettings Receive()
    {
        return new OidcAuthentikProviderSettings (
            AuthorizationFlowSlug: "default-provider-authorization-explicit-consent",
            InvalidationFlowSlug: "default-provider-invalidation-flow",
            RedirectUriMatchingMode: "strict")
        {
            Group = string.Empty,
            Metadata = new AuthentikApplicationMetadataSettings
            {
                Description = string.Empty,
                Icon = string.Empty,
                OpenInNewTab = false,
                Publisher = string.Empty
            },
            TokenSettings = new AuthentikOauth2ProviderTokenSettings
            {
                AccessCodeValidity = "minutes=1",
                AccessTokenValidity = "hours=1",
                RefreshTokenValidity = "days=30"
            }
        };
    }
}