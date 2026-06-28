namespace Moira.Common.Models;

public record GroupProviderSettings(
    string Kind = "",
    IReadOnlyDictionary<string, string>? Attributes = null)
{
    public IReadOnlyDictionary<string, string> Attributes { get; init; } = Attributes ?? new Dictionary<string, string>();
}