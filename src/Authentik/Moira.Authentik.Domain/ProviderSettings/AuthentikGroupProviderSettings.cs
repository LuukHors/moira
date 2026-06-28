namespace Moira.Authentik.Domain.ProviderSettings;

public record AuthentikGroupProviderSettings
{
    public AuthentikGroupAttributeSettings Attributes { get; init; } = new(new Dictionary<string, string>());
}