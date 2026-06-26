namespace Moira.Authentik.Domain.Groups;

public record AuthentikGroupV3(string name, string? pk, IEnumerable<AuthentikUserV3>? users_obj, IReadOnlyDictionary<string, object> attributes, IEnumerable<string> roles, IEnumerable<string> parents);
