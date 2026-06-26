using Microsoft.Extensions.Logging;
using Moira.Authentik.Application.Mappers;
using Moira.Authentik.Application.Ports;
using Moira.Authentik.Domain.Groups;
using Moira.Common.Commands;
using Moira.Common.Exceptions;
using Moira.Common.Mappers;
using Moira.Common.Models;

namespace Moira.Authentik.Application.Handlers;

public class AuthentikGroupHandler(
    IHttpService<AuthentikGroupV3, AuthentikGroupV3, string> httpClient,
    ILogger<AuthentikGroupHandler> logger) : IAuthentikHandler<IdPGroup, AuthentikGroupV3>
{
    private readonly IReadOnlyDictionary<string, object> _defaultAttributes = new Dictionary<string, object> { ["managed-by"] = "moira" } ;

    public async Task<AuthentikGroupV3?> GetAsync(IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.Entity.Status.GroupId))
        {
            logger.LogDebug("Looking up Authentik group by display name {DisplayName}", command.Entity.Spec.DisplayName);
            return await httpClient.GetByNameAsync(command.Entity.Spec.DisplayName, command.Entity.IdPProvider, _defaultAttributes, cancellationToken);
        }

        logger.LogDebug("Looking up Authentik group by group id {GroupId}", command.Entity.Status.GroupId);
        return await httpClient.GetByIdAsync(command.Entity.Status.GroupId, command.Entity.IdPProvider, _defaultAttributes, cancellationToken);
    }

    public async Task<IdPCommandResult<IdPGroup>> CreateAsync(IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        var group = await BuildAuthentikGroupAsync(command, cancellationToken);
        logger.LogInformation("Group does not exist, creating group {DisplayName} with {ParentGroupCount} parent groups", group.name, group.parents.Count());

        var result = await httpClient.CreateAsync(group, command.Entity.IdPProvider, cancellationToken);
        logger.LogInformation("Created Authentik group {DisplayName} with group id {GroupId}", result.name, result.pk);

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
            logger.LogInformation("Group {DisplayName} is already up to date with group id {GroupId}", current.name, current.pk);

            return new IdPCommandResult<IdPGroup>(command.Id, command.Entity.CopyWithNewStatus(new IdPGroupStatus(
                current.pk ?? string.Empty,
                current.name,
                current.parents
            )));
        }

        logger.LogInformation("Group {DisplayName} is not up to date, updating group id {GroupId} with {ParentGroupCount} parent groups", group.name, current.pk, group.parents.Count());

        var result = await httpClient.UpdateAsync(current.pk!, group, command.Entity.IdPProvider, cancellationToken);
        logger.LogInformation("Updated Authentik group {DisplayName} with group id {GroupId}", result.name, result.pk);

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
            logger.LogInformation("Auto delete is disabled, skipping group deletion for group id {GroupId}", command.Entity.Status.GroupId);
            return false;
        }

        if (string.IsNullOrEmpty(command.Entity.Status.GroupId))
        {
            logger.LogInformation("Group does not have a known group id, skipping deletion");
            return false;
        }

        logger.LogInformation("Auto delete is enabled, deleting group id {GroupId}", command.Entity.Status.GroupId);
        var deleted = await httpClient.DeleteAsync(command.Entity.Status.GroupId, command.Entity.IdPProvider, cancellationToken);
        logger.LogInformation("Delete request for group id {GroupId} completed with deleted result {GroupDeleted}", command.Entity.Status.GroupId, deleted);
        return deleted;
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

        logger.LogDebug("Resolving {ParentGroupCount} parent groups", memberOfNames.Count);

        var parentTasks = memberOfNames.Select(async memberOf =>
        {
            var parent = await httpClient.GetByNameAsync(memberOf, command.Entity.IdPProvider, null, cancellationToken)
                ?? throw new IdPException($"Could not find parent group '{memberOf}'", IdPExceptionReason.IdpValidationFailed);

            return parent.pk!;
        });

        return await Task.WhenAll(parentTasks);
    }
}
