using Microsoft.Extensions.Logging;
using Moira.Authentik.Models.V3;
using Moira.Common.Commands;
using Moira.Common.Models;
using Moira.Authentik.HttpService;
using Moira.Authentik.Models.Mappers;
using Moira.Common.Exceptions;
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
        logger.LogInformation("Group does not exist yet, creating..");
        var group = await BuildAuthentikGroupAsync(command, cancellationToken);

        var result = await httpClient.CreateAsync(group, command.Entity.IdPProvider, cancellationToken);

        return new IdPCommandResult<IdPGroup>(command.Id, command.Entity.CopyWithNewStatus(new IdPGroupStatus(
            result.pk!,
            result.name,
            !string.IsNullOrEmpty(result.parent) ? [result.parent] : []
        )));
    }

    public async Task<IdPCommandResult<IdPGroup>> UpdateAsync(AuthentikGroupV3 current, IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        
        if (!ShouldUpdate(current, command))
        {
            logger.LogInformation("Group is already up-to-date");
            
            return new IdPCommandResult<IdPGroup>(command.Id, command.Entity.CopyWithNewStatus(new IdPGroupStatus(
                current.pk ?? string.Empty,
                current.name,
                !string.IsNullOrEmpty(current.parent) ? [current.parent] : []
            )));
        }

        logger.LogInformation("Group is not up-to-date, updating...");

        var group = await BuildAuthentikGroupAsync(command, cancellationToken);

        var result = await httpClient.UpdateAsync(current.pk!, group, command.Entity.IdPProvider, cancellationToken);

        return new IdPCommandResult<IdPGroup>(command.Id, command.Entity.CopyWithNewStatus(new IdPGroupStatus(
            result.pk!,
            result.name,
            !string.IsNullOrEmpty(result.parent) ? [result.parent] : []
        )));
    }

    public async Task<bool> DeleteAsync(IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        if (!command.Entity.Spec.AutoDelete)
        {
            logger.LogInformation("Auto delete is not enabled, will not try to delete group");
            return false;
        }

        if (string.IsNullOrEmpty(command.Entity.Status.GroupId))
        {
            logger.LogInformation("Group does not have a known GroupId, skipping deletion..");
            return false;
        }
        
        logger.LogInformation("Auto delete is enabled, deleting group..");
        return await httpClient.DeleteAsync(command.Entity.Status.GroupId, command.Entity.IdPProvider, cancellationToken);
    }
    
    private static bool ShouldUpdate(AuthentikGroupV3 currentEntity, IdPCommand<IdPGroup> command)
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
    
    private async Task<AuthentikGroupV3> BuildAuthentikGroupAsync(IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        var parentId = await ResolveParentIdAsync(command, cancellationToken);
        return command.Entity.ToAuthentikGroup(parentId, _defaultAttributes);
    }

    private async Task<string> ResolveParentIdAsync(IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        var firstMemberOf = command.Entity.Spec.MemberOf.FirstOrDefault();

        if (string.IsNullOrEmpty(firstMemberOf))
            return string.Empty;
        
        var parent = await httpClient.GetByNameAsync(firstMemberOf, command.Entity.IdPProvider, null, cancellationToken)
            ?? throw new IdPException($"Could not find parent group '{firstMemberOf}'", IdPExceptionReason.IdpValidationFailed);
        
        return parent.pk!;
    }
}
