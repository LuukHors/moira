using Microsoft.Extensions.Logging;
using Moira.Authentik.Handlers;
using Moira.Authentik.Models.V3;
using Moira.Common.Commands;
using Moira.Common.Mappers;
using Moira.Common.Models;
using Moira.Common.Provider;

namespace Moira.Authentik.ProviderAdapters;

public class AuthentikGroupProviderAdapter(
    IAuthentikHandler<IdPGroup, AuthentikGroupV3> handler,
    ILogger<AuthentikGroupProviderAdapter> logger) : AbstractAuthentikProviderAdapter, IProviderAdapter<IdPGroup>
{
    public async Task<IdPCommandResult<IdPGroup>> ExecuteReconcileAsync(IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        if (command.Entity.Spec.MemberOf.Count() > 1)
            logger.LogWarning("Authentik adapter does not support more than one memberOf for groups");
        
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