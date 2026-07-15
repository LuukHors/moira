namespace Moira.Authentik.Domain.Roles;

public sealed record AuthentikPermissionV3(long id, string name, string codename, string model, string app_label, string app_label_verbose, string mode_verbose);