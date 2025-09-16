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
    public async Task<IdPCommandResult<IdPGroup>> ExecuteAsync(IdPCommand<IdPGroup> command)
    {
        try
        {
            logger.LogInformation("[{commandId}][{entityType}][{entityName}] Authentik adapter", command.Id, nameof(IdPGroup), command.Entity.Name);
            var group = await handler.GetAsync(command);
            
            if (group is null)
            {
                return await handler.CreateAsync(command);
            }
            
            return await handler.UpdateAsync(command);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}