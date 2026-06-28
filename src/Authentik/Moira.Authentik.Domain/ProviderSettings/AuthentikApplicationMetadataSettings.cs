namespace Moira.Authentik.Domain.ProviderSettings;

public record AuthentikApplicationMetadataSettings(
    string? Description = null,
    string? Icon = null,
    string? Publisher = null,
    bool OpenInNewTab = false);