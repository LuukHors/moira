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
            logger.LogInformation("[{commandId}][{entityType}][{entityName}] Authentik adapter", command.Id, nameof(IdPGroup), command.Entity.Name);
            var group = await handler.GetAsync(command, cancellationToken);
            
            if (group is null)
            {
                return await handler.CreateAsync(command, cancellationToken);
            }
            
            return await handler.UpdateAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}