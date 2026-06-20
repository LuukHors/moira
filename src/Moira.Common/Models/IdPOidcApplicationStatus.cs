namespace Moira.Common.Models;

public record IdPOidcApplicationStatus(
    string ApplicationId = "",
    string ClientId = "",
    DateTime? LastRotatedAt = null,
    DateTime? NextRotationAt = null,
    IReadOnlyDictionary<string, string>? ProviderResourceIds = null)
{
    public readonly IReadOnlyDictionary<string, string> ProviderResourceIds = ProviderResourceIds ?? new Dictionary<string, string>();
}
