using Microsoft.Extensions.Logging;
using Moira.Authentik.Authentication;
using Moira.Authentik.Models.V3;
using Moira.Common.Commands;
using Moira.Common.Models;

namespace Moira.Authentik.Handlers;

public class AuthentikGroupHandler(
    IAuthentikAuthenticationService tokenService,
    ILogger<AuthentikGroupHandler> logger) : IAuthentikHandler<IdPGroup, AuthentikGroupV3>
{
    public async Task<AuthentikGroupV3?> GetAsync(IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        logger.LogInformation("GetAsync");
        var token = await tokenService.AcquireTokenAsync(command.Entity.IdPProvider, cancellationToken);
        return null;
    }

    public Task<IdPCommandResult<IdPGroup>> CreateAsync(IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        logger.LogInformation("CreateAsync");
        var idpGroup = new IdPGroup(
            command.Entity.Namespace,
            command.Entity.Name,
            command.Entity.IdPProvider,
            command.Entity.Spec,
            new IdPGroupStatus(Guid.NewGuid().ToString()));

        return Task.FromResult(new IdPCommandResult<IdPGroup>(command.Id, idpGroup, IdPCommandResultStatus.Success));
    }

    public Task<IdPCommandResult<IdPGroup>> UpdateAsync(IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        logger.LogInformation("UpdateAsync");
        return Task.FromResult(new IdPCommandResult<IdPGroup>(command.Id, command.Entity, IdPCommandResultStatus.Success));
    }
}