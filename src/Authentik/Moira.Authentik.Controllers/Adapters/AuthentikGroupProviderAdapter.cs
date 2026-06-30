using Moira.Authentik.Application.Handlers;
using Moira.Authentik.Domain.Groups;
using Moira.Common.Abstractions;
using Moira.Common.Abstractions.Commands;
using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Controllers.Adapters;

public class AuthentikGroupProviderAdapter(
    IAuthentikHandler<IdPGroup, AuthentikGroupV3> handler) : AbstractAuthentikProviderAdapter, IProviderAdapter<IdPGroup>
{
    public async Task<IdPCommandResult<IdPGroup>> ExecuteReconcileAsync(IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        var group = await handler.GetAsync(command, cancellationToken);
        if (group is not null)
        {
            return await handler.UpdateAsync(group, command, cancellationToken);
        }

        return await handler.CreateAsync(command, cancellationToken);
    }

    public async Task<bool> ExecuteDeleteAsync(IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
        => await handler.DeleteAsync(command, cancellationToken);
}
