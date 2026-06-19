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
            result.parents
        )));
    }

    public async Task<IdPCommandResult<IdPGroup>> UpdateAsync(AuthentikGroupV3 current, IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        var group = await BuildAuthentikGroupAsync(command, cancellationToken);
        
        if (!ShouldUpdate(current, group))
        {
            logger.LogInformation("Group is already up-to-date");
            
            return new IdPCommandResult<IdPGroup>(command.Id, command.Entity.CopyWithNewStatus(new IdPGroupStatus(
                current.pk ?? string.Empty,
                current.name,
                current.parents
            )));
        }

        logger.LogInformation("Group is not up-to-date, updating...");

        var result = await httpClient.UpdateAsync(current.pk!, group, command.Entity.IdPProvider, cancellationToken);

        return new IdPCommandResult<IdPGroup>(command.Id, command.Entity.CopyWithNewStatus(new IdPGroupStatus(
            result.pk!,
            result.name,
            result.parents
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
    
    private static bool ShouldUpdate(AuthentikGroupV3 currentEntity, AuthentikGroupV3 desiredEntity)
    {
        if (!desiredEntity.name.Equals(currentEntity.name))
            return true;

        var desiredParentIds = desiredEntity.parents.ToHashSet();
        var currentParentIds = currentEntity.parents.ToHashSet();

        return !desiredParentIds.SetEquals(currentParentIds);
    }
    
    private async Task<AuthentikGroupV3> BuildAuthentikGroupAsync(IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        var parentIds = await ResolveParentIdsAsync(command, cancellationToken);
        return command.Entity.ToAuthentikGroup(parentIds, _defaultAttributes);
    }

    private async Task<IEnumerable<string>> ResolveParentIdsAsync(IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        var memberOfNames = command.Entity.Spec.MemberOf
            .Where(memberOf => !string.IsNullOrEmpty(memberOf))
            .Distinct()
            .ToList();

        var parentTasks = memberOfNames.Select(async memberOf =>
        {
            var parent = await httpClient.GetByNameAsync(memberOf, command.Entity.IdPProvider, null, cancellationToken)
                ?? throw new IdPException($"Could not find parent group '{memberOf}'", IdPExceptionReason.IdpValidationFailed);

            return parent.pk!;
        });

        return await Task.WhenAll(parentTasks);
    }
}
