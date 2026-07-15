using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Application.Models.Role;

public record AuthentikRoleModel(
    string Namespace,
    string Name,
    IdPProvider IdPProvider,
    AuthentikRoleSpec Spec,
    AuthentikRoleStatus Status) : IdPEntity(Namespace, Name, IdPProvider);