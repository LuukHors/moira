namespace Moira.Common.Abstractions.Models;

public record IdPGroupSpec(
    string DisplayName,
    IEnumerable<string> MemberOf,
    bool AutoDelete,
    IdpProviderSpecificSettings? ProviderSettings = null);
