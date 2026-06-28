using Moira.Authentik.Domain.ProviderSettings;
using Moira.Common.Models;

namespace Moira.Authentik.Application.Mappers;

public static class OidcProviderSettingsMapper
{
    public static OidcAuthentikProviderSettings ToAuthentikSettings(this OidcProviderSettings providerSettings)
    {
        return new OidcAuthentikProviderSettings();
    }
}