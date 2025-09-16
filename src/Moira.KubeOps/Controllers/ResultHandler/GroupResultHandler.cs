using KubeOps.Abstractions.Queue;
using Microsoft.Extensions.Logging;
using Moira.Common.Commands;
using Moira.Common.Models;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.ResultHandler;

public class GroupResultHandler(
    EntityRequeue<Group> entityRequeue,
    ILogger<GroupResultHandler> logger) : IResultHandler<Group, IdPGroup>
{
    public Task HandleAsync(Group entity, IdPCommandResult<IdPGroup> result, CancellationToken cancellationToken)
    {
        logger.LogInformation("[{commandId}][{entityType}][{entityName}] GroupResultHandler", result.Id, nameof(IdPGroup), result.Entity.Name);
        entityRequeue(entity, TimeSpan.FromSeconds(20));
        return Task.CompletedTask;
    }
}