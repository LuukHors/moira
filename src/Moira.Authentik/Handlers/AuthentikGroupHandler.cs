using Microsoft.Extensions.Logging;
using Moira.Authentik.Models.V3;
using Moira.Common.Commands;
using Moira.Common.Models;
using Moira.Authentik.HttpService;
using Moira.Authentik.Models.Mappers;
using Moira.Common.Mappers;

namespace Moira.Authentik.Handlers;

public class AuthentikGroupHandler(
    IAuthentikHttpService<IdPGroup, AuthentikGroupV3> httpService,
    IHttpService<AuthentikGroupV3, AuthentikGroupV3, string> httpClient,
    ILogger<AuthentikGroupHandler> logger) : IAuthentikHandler<IdPGroup, AuthentikGroupV3>
{
    private readonly IReadOnlyDictionary<string, object> _defaultAttributes = new Dictionary<string, object> { ["managed-by"] = "moira" } ;
    
    public async Task<AuthentikGroupV3?> GetAsync(IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.Entity.Status.GroupId))
        {
            return await httpClient.GetByNameAsync(command.Entity.Spec.DisplayName, command.Entity.IdPProvider, _defaultAttributes, cancellationToken);
        }
        
        return await httpClient.GetByIdAsync(command.Entity.Status.GroupId, command.Entity.IdPProvider, _defaultAttributes, cancellationToken);
    }

    public async Task<IdPCommandResult<IdPGroup>> CreateAsync(IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        logger.LogInformation("[{commandId}][{entityType}][{entityName}] Group does not exist yet, creating..", command.Id, nameof(IdPGroup), command.Entity.Name);
        
        var parentGroups = await httpClient.ListAsync(
            command.Entity.Spec.MemberOf.FirstOrDefault(),
            _defaultAttributes,
            command.Entity.IdPProvider, 
            null, 
            cancellationToken);
        
        var group = command.Entity.ToAuthentikGroup(parentGroups.Results.FirstOrDefault()?.pk ?? string.Empty, _defaultAttributes);
        var result = await httpClient.CreateAsync(group, command.Entity.IdPProvider, cancellationToken);

        return new IdPCommandResult<IdPGroup>(command.Id, command.Entity.CopyWithNewStatus(new IdPGroupStatus(
            result.pk!,
            result.name
        )));
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

        if (command.Entity.Spec.MemberOf.Any() && string.IsNullOrEmpty(currentEntity.parent))
            return true;
            
        return false;
    }
}