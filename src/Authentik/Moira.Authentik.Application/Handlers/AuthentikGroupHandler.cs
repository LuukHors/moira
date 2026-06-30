using Microsoft.Extensions.Logging;
using Moira.Authentik.Application.Builders;
using Moira.Authentik.Application.Ports;
using Moira.Authentik.Domain.Groups;
using Moira.Common.Abstractions;
using Moira.Common.Abstractions.Commands;
using Moira.Common.Abstractions.Mappers;
using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Application.Handlers;

public class AuthentikGroupHandler(
    IAuthentikRepository<AuthentikGroupV3, AuthentikGroupV3, string> groupRepository,
    IAuthentikGroupBuilder groupBuilder,
    IUpdateChecker<AuthentikGroupV3, AuthentikGroupV3> updateChecker,
    ILogger<AuthentikGroupHandler> logger) : IAuthentikHandler<IdPGroup, AuthentikGroupV3>
{
    private static readonly IReadOnlyDictionary<string, object> DefaultAttributes = new Dictionary<string, object> { ["managed-by"] = "moira" };

    public async Task<AuthentikGroupV3?> GetAsync(IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.Entity.Status.GroupId))
        {
            logger.LogDebug("Looking up Authentik group by display name {DisplayName}", command.Entity.Spec.DisplayName);
            return await groupRepository.GetByNameAsync(command.Entity.Spec.DisplayName, command.Entity.IdPProvider, DefaultAttributes, cancellationToken);
        }

        logger.LogDebug("Looking up Authentik group by group id {GroupId}", command.Entity.Status.GroupId);
        return await groupRepository.GetByIdAsync(command.Entity.Status.GroupId, command.Entity.IdPProvider, DefaultAttributes, cancellationToken);
    }

    public async Task<IdPCommandResult<IdPGroup>> CreateAsync(IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        var group = await groupBuilder.BuildAsync(command, cancellationToken);
        logger.LogInformation("Group does not exist, creating group {DisplayName} with {ParentGroupCount} parent groups", group.name, group.parents.Count());

        var result = await groupRepository.CreateAsync(group, command.Entity.IdPProvider, cancellationToken);
        logger.LogInformation("Created Authentik group {DisplayName} with group id {GroupId}", result.name, result.pk);

        return new IdPCommandResult<IdPGroup>(command.Id, command.Entity.CopyWithNewStatus(new IdPGroupStatus(
            result.pk!,
            result.name,
            result.parents
        )));
    }

    public async Task<IdPCommandResult<IdPGroup>> UpdateAsync(AuthentikGroupV3 current, IdPCommand<IdPGroup> command, CancellationToken cancellationToken)
    {
        var group = await groupBuilder.BuildAsync(command, cancellationToken);

        if (!updateChecker.ShouldUpdate(group, current))
        {
            logger.LogInformation("Group {DisplayName} is already up to date with group id {GroupId}", current.name, current.pk);

            return new IdPCommandResult<IdPGroup>(command.Id, command.Entity.CopyWithNewStatus(new IdPGroupStatus(
                current.pk ?? string.Empty,
                current.name,
                current.parents
            )));
        }

        logger.LogInformation("Group {DisplayName} is not up to date, updating group id {GroupId} with {ParentGroupCount} parent groups", group.name, current.pk, group.parents.Count());

        var result = await groupRepository.UpdateAsync(current.pk!, group, command.Entity.IdPProvider, cancellationToken);
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
        var deleted = await groupRepository.DeleteAsync(command.Entity.Status.GroupId, command.Entity.IdPProvider, cancellationToken);
        logger.LogInformation("Delete request for group id {GroupId} completed with deleted result {GroupDeleted}", command.Entity.Status.GroupId, deleted);
        return deleted;
    }
}