using Microsoft.Extensions.Logging;
using Moira.Authentik.Models.V3;
using Moira.Common.Commands;
using Moira.Common.Models;
using Moira.Authentik.HttpService;
using Moira.Authentik.Models.Mappers;
using Moira.Common.Mappers;

namespace Moira.Authentik.Handlers;

public class AuthentikGroupHandler(
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

        var group = await ConvertToAuthentikGroup(command, cancellationToken);

        var result = await httpClient.CreateAsync(group, command.Entity.IdPProvider, cancellationToken);

        return new IdPCommandResult<IdPGroup>(command.Id, command.Entity.CopyWithNewStatus(new IdPGroupStatus(
            result.pk!,
            result.name,
            !string.IsNullOrEmpty(result.parent) ? [result.parent] : []
        )));
    }

    public async Task<IdPCommandResult<IdPGroup>> UpdateAsync(AuthentikGroupV3 currentEntity, IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        if (!ShouldUpdateGroup(currentEntity, command))
        {
            logger.LogInformation("[{commandId}][{entityType}][{entityName}] Group is already up-to-date", command.Id, nameof(IdPGroup), command.Entity.Name);
            
            return new IdPCommandResult<IdPGroup>(command.Id, command.Entity.CopyWithNewStatus(new IdPGroupStatus(
                currentEntity.pk ?? string.Empty,
                currentEntity.name,
                !string.IsNullOrEmpty(currentEntity.parent) ? [currentEntity.parent] : []
            )));
        }

        logger.LogInformation("[{commandId}][{entityType}][{entityName}] Group is not up-to-date, updating...", command.Id, nameof(IdPGroup), command.Entity.Name);

        var group = await ConvertToAuthentikGroup(command, cancellationToken);

        var result = await httpClient.UpdateAsync(command.Entity.Status.GroupId, group, command.Entity.IdPProvider, cancellationToken);

        return new IdPCommandResult<IdPGroup>(command.Id, command.Entity.CopyWithNewStatus(new IdPGroupStatus(
            result.pk!,
            result.name,
            !string.IsNullOrEmpty(result.parent) ? [result.parent] : []
        )));
    }
    
    public async Task<bool> DeleteAsync(IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
        => await httpClient.DeleteAsync(command.Entity.Status.GroupId, command.Entity.IdPProvider, cancellationToken);
    
    private static bool ShouldUpdateGroup(AuthentikGroupV3 currentEntity, IdPCommand<IdPGroup> command)
    {
        var hasMemberOf = command.Entity.Spec.MemberOf.Any();
        
        if (!command.Entity.Spec.DisplayName.Equals(currentEntity.name))
            return true;

        if (hasMemberOf && string.IsNullOrEmpty(currentEntity.parent))
            return true;

        if (!hasMemberOf && !string.IsNullOrEmpty(currentEntity.parent))
            return true;
            
        return !string.IsNullOrEmpty(command.Entity.Spec.MemberOf.FirstOrDefault()) 
                    && !command.Entity.Status.MemberOfGroupIds.Contains(currentEntity.parent);
    }
    
    private async Task<AuthentikGroupV3> ConvertToAuthentikGroup(IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        AuthentikGroupV3? parentGroup = null;
        var firstMemberOf = command.Entity.Spec.MemberOf.FirstOrDefault();
        
        if (!string.IsNullOrEmpty(firstMemberOf))
        {
            var parentGroups = await httpClient.ListAsync(
                firstMemberOf,
                null,
                command.Entity.IdPProvider, 
                null, 
                cancellationToken);

            parentGroup = parentGroups.Results.FirstOrDefault();
        }
        
        var group = command.Entity.ToAuthentikGroup(
            parentGroup?.pk ?? string.Empty,
            _defaultAttributes);
        
        return group;
    }
}