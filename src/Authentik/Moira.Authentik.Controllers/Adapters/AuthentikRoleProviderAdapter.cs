using Moira.Authentik.Application.Handlers;
using Moira.Authentik.Application.Models.Role;
using Moira.Authentik.Domain.Roles;
using Moira.Common.Abstractions;
using Moira.Common.Abstractions.Commands;

namespace Moira.Authentik.Controllers.Adapters;

public class AuthentikRoleProviderAdapter(
    IAuthentikHandler<AuthentikRoleModel, AuthentikRoleV3> handler) : IProviderAdapter<AuthentikRoleModel>
{
    public async Task<IdPCommandResult<AuthentikRoleModel>> ExecuteReconcileAsync(IdPCommand<AuthentikRoleModel> command, CancellationToken cancellationToken)
    {
        var role = await handler.GetAsync(command, cancellationToken);
        if (role is not null)
        {
            return await handler.UpdateAsync(role, command, cancellationToken);
        }
        
        return await handler.CreateAsync(command, cancellationToken);
    }

    public async Task<bool> ExecuteDeleteAsync(IdPCommand<AuthentikRoleModel> command, CancellationToken cancellationToken)
        => await handler.DeleteAsync(command, cancellationToken);
}
