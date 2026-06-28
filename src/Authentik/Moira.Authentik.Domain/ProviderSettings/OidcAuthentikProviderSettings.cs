namespace Moira.Authentik.Domain.ProviderSettings;

public record OidcAuthentikProviderSettings(string? Group = null)
{
    public AuthentikApplicationMetadataSettings Metadata { get; init; } = new();
    public AuthentikOauth2ProviderTokenSettings TokenSettings { get; init; } = new();
}