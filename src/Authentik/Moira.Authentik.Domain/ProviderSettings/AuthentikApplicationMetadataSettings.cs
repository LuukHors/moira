namespace Moira.Authentik.Domain.ProviderSettings;

public record AuthentikApplicationMetadataSettings(
    string Description = "",
    string Icon = "",
    string Publisher = "",
    bool OpenInNewTab = false);