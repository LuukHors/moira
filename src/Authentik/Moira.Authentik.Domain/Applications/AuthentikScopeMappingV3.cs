namespace Moira.Authentik.Domain.Applications;

public record AuthentikScopeMappingV3(
    string pk,
    string name,
    string scope_name);
