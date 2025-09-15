namespace Moira.Common.Models;

public record IdPGroup(
    string Namespace,
    string Name,
    IdPProvider IdPProvider,
    string DisplayName,
    IEnumerable<string> MemberOf) : IdPEntity(Namespace, Name, IdPProvider);