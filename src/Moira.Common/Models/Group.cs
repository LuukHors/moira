namespace Moira.Common.Models;

public record IdPGroup(
    string Namespace,
    string Name,
    string DisplayName,
    IEnumerable<string> MemberOf) : IdPEntity(Namespace, Name);