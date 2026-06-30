namespace Moira.Common.Abstractions.Models;

public record IdpProviderSpecificSettings(IReadOnlyDictionary<string, string>? Values = null)
{
    public IReadOnlyDictionary<string, string> Values { get; init; } = Values ?? new Dictionary<string, string>();
    
    public string GetValueOrDefault(string key, string defaultValue)
    {
        return Values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : defaultValue;
    }
}