namespace Moira.Authentik.Models.V3;

public record AuthentikGroupV3(string name, string? pk, IEnumerable<AuthentikUserV3> users_obj, IDictionary<string, object> attributes, IEnumerable<string> roles, string parent = "");