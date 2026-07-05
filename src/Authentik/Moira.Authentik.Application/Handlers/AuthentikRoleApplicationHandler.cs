using Microsoft.Extensions.Logging;
using Moira.Authentik.Application.Models.Role;
using Moira.Authentik.Application.Ports;
using Moira.Authentik.Domain.Roles;
using Moira.Common.Abstractions;
using Moira.Common.Abstractions.Commands;

namespace Moira.Authentik.Application.Handlers;

public class AuthentikRoleApplicationHandler(
    IAuthentikRepository<AuthentikRoleV3, AuthentikRoleV3, string> roleRepository,
    IUpdateChecker<AuthentikRoleV3, AuthentikRoleV3> updateChecker,
    ILogger<AuthentikRoleApplicationHandler> logger) : IAuthentikHandler<AuthentikRoleModel, AuthentikRoleV3>
{
    public async Task<AuthentikRoleV3?> GetAsync(IdPCommand<AuthentikRoleModel> command, CancellationToken cancellationToken)
    {
        var roleId = command.Entity.Status.RoleId;
        if (string.IsNullOrEmpty(roleId))
        {
            return await roleRepository.GetByNameAsync(command.Entity.Spec.DisplayName, command.Entity.IdPProvider, attributes: null, cancellationToken);
        }

        return await roleRepository.GetByIdAsync(roleId, command.Entity.IdPProvider, attributes: null, cancellationToken);
    }

    public async Task<IdPCommandResult<AuthentikRoleModel>> CreateAsync(IdPCommand<AuthentikRoleModel> command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Authentik role does not exist, creating role {DisplayName}", command.Entity.Spec.DisplayName);
        var role = new AuthentikRoleV3(pk: null, name: command.Entity.Spec.DisplayName);
        
        var createdRole = await roleRepository.CreateAsync(role, command.Entity.IdPProvider, cancellationToken);
        logger.LogInformation("Created Authentik role {DisplayName} with role id {RoleId}", createdRole.name, createdRole.pk);

        return new IdPCommandResult<AuthentikRoleModel>(command.Id, command.Entity with { Status = new AuthentikRoleStatus(createdRole.pk!) });
    }

    public async Task<IdPCommandResult<AuthentikRoleModel>> UpdateAsync(AuthentikRoleV3 current, IdPCommand<AuthentikRoleModel> command, CancellationToken cancellationToken)
    {
        var role = new AuthentikRoleV3(pk: null, name: command.Entity.Spec.DisplayName);

        if (!updateChecker.ShouldUpdate(role, current))
        {
            logger.LogInformation("Role {DisplayName} is already up to date with role id {RoleId}", current.name, current.pk);
            return new IdPCommandResult<AuthentikRoleModel>(command.Id, command.Entity with { Status = new AuthentikRoleStatus(current.pk!) });
        }

        logger.LogInformation("Role {DisplayName} is not up to date, updating role id {GroupId}", role.name, current.pk);

        var result = await roleRepository.UpdateAsync(current.pk!, role, command.Entity.IdPProvider, cancellationToken);
        logger.LogInformation("Updated Authentik role {DisplayName} with role id {RoleId}", result.name, result.pk);

        return new IdPCommandResult<AuthentikRoleModel>(command.Id, command.Entity with { Status = new AuthentikRoleStatus(current.pk!) }); 
    }

    public async Task<bool> DeleteAsync(IdPCommand<AuthentikRoleModel> command, CancellationToken cancellationToken)
        => await roleRepository.DeleteAsync(command.Entity.Status.RoleId, command.Entity.IdPProvider, cancellationToken);
}