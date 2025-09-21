using Microsoft.Extensions.Logging;
using Moira.Authentik.Authentication;
using Moira.Authentik.Models.V3;
using Moira.Common.Commands;
using Moira.Common.Models;
using Moira.Authentik.HttpService;
using Moira.Common.Mappers;

namespace Moira.Authentik.Handlers;

public class AuthentikGroupHandler(
    IAuthentikHttpService<IdPGroup, AuthentikGroupV3> httpService,
    IAuthentikAuthenticationService tokenService,
    ILogger<AuthentikGroupHandler> logger) : IAuthentikHandler<IdPGroup, AuthentikGroupV3>
{
    public async Task<AuthentikGroupV3?> GetAsync(IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        return await httpService.GetAsync(command.Entity, cancellationToken);
    }

    public async Task<IdPCommandResult<IdPGroup>> CreateAsync(IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        logger.LogInformation("[{commandId}][{entityType}][{entityName}] Group does not exist yet, creating..", command.Id, nameof(IdPGroup), command.Entity.Name);
        
        var result = await httpService.CreateAsync(command.Entity, cancellationToken);

        return new IdPCommandResult<IdPGroup>(command.Id, result);
    }

    public async Task<IdPCommandResult<IdPGroup>> UpdateAsync(AuthentikGroupV3 currentEntity, IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        if (!ShouldUpdateGroup(currentEntity, command))
        {
            logger.LogInformation("[{commandId}][{entityType}][{entityName}] Group is already up-to-date", command.Id, nameof(IdPGroup), command.Entity.Name);
            
            return new IdPCommandResult<IdPGroup>(command.Id, command.Entity.CopyWithNewStatus(new IdPGroupStatus(
                currentEntity.pk ?? string.Empty,
                currentEntity.name
            )));
        }

        logger.LogInformation("[{commandId}][{entityType}][{entityName}] Group is not up-to-date, updating...", command.Id, nameof(IdPGroup), command.Entity.Name);
        var idPGroup = await httpService.UpdateAsync(command.Entity, cancellationToken);
        
        return new IdPCommandResult<IdPGroup>(command.Id, idPGroup);
    }

    private static bool ShouldUpdateGroup(AuthentikGroupV3 currentEntity, IdPCommand<IdPGroup> command)
    {
        if (!command.Entity.Spec.DisplayName.Equals(currentEntity.name, StringComparison.OrdinalIgnoreCase))
            return true;

        // if (command.Entity.Spec.MemberOf.Any() && currentEntity.parent is null)
        //     return true;
            
        return false;
    }
}