namespace Moira.Authentik.Domain.Applications;

public record AuthentikApplicationV3(
    string name,
    string slug,
    string? pk,
    int? provider,
    string? meta_launch_url,
    string? meta_icon,
    string? meta_icon_url,
    string? meta_description,
    string? meta_publisher,
    bool open_in_new_tab,
    string? group);
