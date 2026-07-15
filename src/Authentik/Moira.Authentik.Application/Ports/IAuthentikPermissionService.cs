using Moira.Authentik.Domain;
using Moira.Authentik.Domain.Roles;
using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Application.Ports;

public interface IAuthentikPermissionService
{
    Task<AuthentikPageResult<AuthentikPermissionV3>> GetPermissionsAsync(string roleId, IdPProvider provider, CancellationToken cancellationToken);
    
    Task<bool> UnassignPermissionsAsync(string roleId, IEnumerable<string> permissions, IdPProvider provider, CancellationToken cancellationToken);
    
    Task<bool> AssignPermissionsAsync(string roleId, IEnumerable<string> permissions, IdPProvider provider, CancellationToken cancellationToken);
}