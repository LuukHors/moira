namespace Moira.Authentik.Models.V3;

public record AuthentikGroupV3(string name, bool is_superuser, string? uuid, IEnumerable<int> users, IDictionary<string, object> attributes, IEnumerable<string> roles);