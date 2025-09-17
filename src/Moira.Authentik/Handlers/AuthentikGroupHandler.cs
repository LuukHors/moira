using Microsoft.Extensions.Logging;
using Moira.Authentik.Models.V3;
using Moira.Common.Commands;
using Moira.Common.Models;

namespace Moira.Authentik.Handlers;

public class AuthentikGroupHandler(ILogger<AuthentikGroupHandler> logger) : IAuthentikHandler<IdPGroup, AuthentikGroupV3>
{
    public Task<AuthentikGroupV3?> GetAsync(IdPCommand<IdPGroup> command)
    {
        if(command.Entity.IdPProvider.Type.Equals("Authentik", StringComparison.InvariantCultureIgnoreCase))
        {

        }

        logger.LogInformation("GetAsync");
        return Task.FromResult<AuthentikGroupV3?>(null);
    }

    public Task<IdPCommandResult<IdPGroup>> CreateAsync(IdPCommand<IdPGroup> command)
    {
        logger.LogInformation("CreateAsync");
        var idpGroup = new IdPGroup(
            command.Entity.Namespace,
            command.Entity.Name,
            command.Entity.IdPProvider,
            command.Entity.Spec,
            new IdPGroupStatus(Guid.NewGuid().ToString()));

        return Task.FromResult(new IdPCommandResult<IdPGroup>(command.Id, idpGroup));
    }

    public Task<IdPCommandResult<IdPGroup>> UpdateAsync(IdPCommand<IdPGroup> command)
    {
        logger.LogInformation("UpdateAsync");
        return Task.FromResult(new IdPCommandResult<IdPGroup>(command.Id, command.Entity));
    }
}