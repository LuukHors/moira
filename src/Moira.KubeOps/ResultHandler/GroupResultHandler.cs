using KubeOps.Abstractions.Queue;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using Moira.Common.Commands;
using Moira.Common.Models;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.ResultHandler;

public class GroupResultHandler(
    IKubernetesClient client,
    EntityRequeue<Group> entityRequeue,
    ILogger<GroupResultHandler> logger) : IResultHandler<Group, IdPGroup>
{
    public Task HandleAsync(Group entity, IdPCommandResult<IdPGroup> result, CancellationToken cancellationToken) => result.Status switch
    {
        IdPCommandResultStatus.Success => Success(entity, result, cancellationToken),
        _ => Failed(entity, result, cancellationToken)
    };
    //{
    //    result.Status switch
    //    {
    //        IdPCommandResultStatus.Success => await Success(),
    //        _ => await Failed()
    //    };

    //    logger.LogDebug("[{commandId}][{entityType}][{entityName}] Entering GroupResultHandler", result.Id, nameof(IdPGroup), result.Entity.Name);

    //    if(!entity.Status.GroupId.Equals(result.Entity.Status.GroupId, StringComparison.CurrentCultureIgnoreCase))
    //    {
    //        entity.Status.GroupId = result.Entity.Status.GroupId;
    //    }

    //    await client.UpdateStatusAsync(entity, cancellationToken);

    //    logger.LogDebug("[{commandId}][{entityType}][{entityName}] Requeue'ing entity", result.Id, nameof(IdPGroup), result.Entity.Name);
    //    entityRequeue(entity, TimeSpan.FromSeconds(20));
    //    logger.LogDebug("[{commandId}][{entityType}][{entityName}] Done handling GroupResultHandler", result.Id, nameof(IdPGroup), result.Entity.Name);
    //}

    private async Task Success(Group entity, IdPCommandResult<IdPGroup> result, CancellationToken cancellationToken)
    {

    }

    private async Task Failed(Group entity, IdPCommandResult<IdPGroup> result, CancellationToken cancellationToken)
    {

    }
}