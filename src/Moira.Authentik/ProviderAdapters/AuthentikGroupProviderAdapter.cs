using Microsoft.Extensions.Logging;
using Moira.Authentik.Handlers;
using Moira.Authentik.Models.V3;
using Moira.Common.Commands;
using Moira.Common.Models;
using Moira.Common.Provider;

namespace Moira.Authentik.ProviderAdapters;

public class AuthentikGroupProviderAdapter(
    IAuthentikHandler<IdPGroup, AuthentikGroupV3> handler,
    ILogger<AuthentikGroupProviderAdapter> logger) : AbstractAuthentikProviderAdapter, IProviderAdapter<IdPGroup>
{
    public async Task<IdPCommandResult<IdPGroup>> ExecuteAsync(IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        try
        {
            if (command.Entity.Spec.MemberOf.Count() > 1)
                logger.LogWarning("[{commandId}][{entityType}][{entityName}] Authentik adapter does not support more than one memberOf for groups", command.Id, nameof(IdPGroup), command.Entity.Name);
            
            var group = await handler.GetAsync(command, cancellationToken);
            
            if (group is null)
            {
                return await handler.CreateAsync(command, cancellationToken);
            }
            
            return await handler.UpdateAsync(group, command, cancellationToken);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}