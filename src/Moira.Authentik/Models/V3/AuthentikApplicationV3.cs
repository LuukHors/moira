namespace Moira.Authentik.Models.V3;

public record AuthentikApplicationV3(
    string name,
    string slug,
    string? pk,
    string provider,
    string? launch_url);
