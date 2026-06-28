using Moira.Authentik.Domain.ProviderSettings;
using Moira.Common.Models;

namespace Moira.Authentik.Application.Mappers;

public static class GroupProviderSettingsMapper
{
    public static AuthentikGroupProviderSettings ToAuthentikSettings(this GroupProviderSettings providerSettings)
    {
        return new AuthentikGroupProviderSettings
        {
            Attributes = new AuthentikGroupAttributeSettings(providerSettings.Attributes)
        };
    }
}