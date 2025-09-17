namespace Moira.Common.Models;

public record IdPGroupSpec(
    string DisplayName,
    IEnumerable<string> MemberOf);
