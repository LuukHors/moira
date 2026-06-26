namespace Moira.Authentik.Domain.Applications;

public record AuthentikApplicationV3(
    string name,
    string slug,
    string? pk,
    int? provider,
    string? launch_url);
