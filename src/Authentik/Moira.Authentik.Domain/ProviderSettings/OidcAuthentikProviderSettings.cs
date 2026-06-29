namespace Moira.Authentik.Domain.ProviderSettings;

public record OidcAuthentikProviderSettings(string? InvalidationFlowSlug, string? AuthorizationFlowSlug, string? RedirectUriMatchingMode, string? Group = null)
{
    public AuthentikApplicationMetadataSettings Metadata { get; init; } = new();
    public AuthentikOauth2ProviderTokenSettings TokenSettings { get; init; } = new();
}