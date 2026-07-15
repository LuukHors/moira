namespace Moira.Authentik.Application.Models.Group;

public record AuthentikGroupSpec(
    string DisplayName,
    IEnumerable<string> MemberOf,
    IEnumerable<string> Roles,
    bool AutoDelete);