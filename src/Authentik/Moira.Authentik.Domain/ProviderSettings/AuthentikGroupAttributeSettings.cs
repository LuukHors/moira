namespace Moira.Authentik.Domain.ProviderSettings;

public record AuthentikGroupAttributeSettings(IReadOnlyDictionary<string, string> Values);