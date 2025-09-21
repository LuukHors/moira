using KubeOps.Abstractions.Queue;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Logging;
using Moira.Common.Exceptions;
using Moira.Common.Models;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.ResultHandler;

public class GroupResultHandler(
    IKubernetesClient client,
    EntityRequeue<Group> entityRequeue,
    ILogger<GroupResultHandler> logger) : IResultHandler<Group, IdPGroup>
{
    public Task HandleAsync(Group entity, CancellationToken cancellationToken, IdPGroup? idpEntity = null, IdPException? exception = null)
        => idpEntity is null
            ? HandleExceptionResult(entity, exception!, cancellationToken) 
            : HandleSuccessResult(entity, idpEntity, cancellationToken);

    private async Task HandleExceptionResult(Group entity, IdPException exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "testing.");
        entity.Status.ObservedGeneration = entity.Metadata.Generation;
        entity.Status.Synced = false;

        await client.UpdateStatusAsync(entity, cancellationToken);
        
        entityRequeue(entity, TimeSpan.FromSeconds(20));
    }

    private async Task HandleSuccessResult(Group entity, IdPGroup group, CancellationToken cancellationToken)
    {
        entity.Status.ObservedGeneration = entity.Metadata.Generation;
        entity.Status.DisplayName = group.Status.DisplayName;
        entity.Status.GroupId = group.Status.GroupId;
        entity.Status.ObservedGeneration = entity.Metadata.Generation;
        entity.Status.Synced = true;

        await client.UpdateStatusAsync(entity, cancellationToken);
        
        entityRequeue(entity, TimeSpan.FromSeconds(20));
    }
}