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
            logger.LogWarning("[{commandId}][{entityType}][{entityName}] Authentik adapter does not support more than one memberOf for groups", command.Id, nameof(IdPGroup), command.Entity.Name);
        
        var group = await handler.GetAsync(command, cancellationToken);

        // if (group is not null && string.IsNullOrEmpty(command.Entity.Status.GroupId)) //this means that the group existed previously or had a attribute added.
        // {
        //     logger.LogInformation("[{commandId}][{entityType}][{entityName}] Group already exists and contains the correct attributes, setting status..", command.Id, nameof(IdPGroup), command.Entity.Name);
        //     return new IdPCommandResult<IdPGroup>(command.Id, command.Entity.CopyWithNewStatus(new IdPGroupStatus(
        //         group.pk!,
        //         group.name,
        //         !string.IsNullOrEmpty(group.parent) ? [group.parent] : []
        //     )));
        // }
        
        if (group is null)
        {
            return await handler.CreateAsync(command, cancellationToken);
        }
        
        return await handler.UpdateAsync(group, command, cancellationToken);
    }

    public Task<bool> ExecuteDeleteAsync(IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}