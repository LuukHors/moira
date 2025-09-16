using Microsoft.Extensions.Logging;
using Moira.Authentik.Models.V3;
using Moira.Common.Commands;
using Moira.Common.Models;

namespace Moira.Authentik.Handlers;

public class AuthentikGroupHandler(ILogger<AuthentikGroupHandler> logger) : IAuthentikHandler<IdPGroup, AuthentikGroupV3>
{
    public Task<AuthentikGroupV3?> GetAsync(IdPCommand<IdPGroup> command)
    {
        logger.LogInformation("GetAsync");
        return Task.FromResult<AuthentikGroupV3?>(null);
    }

    public Task<IdPCommandResult<IdPGroup>> CreateAsync(IdPCommand<IdPGroup> command)
    {
        logger.LogInformation("CreateAsync");
        return Task.FromResult(new IdPCommandResult<IdPGroup>(command.Id, command.Entity));
    }

    public Task<IdPCommandResult<IdPGroup>> UpdateAsync(IdPCommand<IdPGroup> command)
    {
        logger.LogInformation("UpdateAsync");
        return Task.FromResult(new IdPCommandResult<IdPGroup>(command.Id, command.Entity));
    }
}