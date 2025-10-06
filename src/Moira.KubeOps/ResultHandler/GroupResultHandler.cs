using System.Net.Http.Headers;
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
        logger.LogError(exception, "");
        
        entity.Status.ObservedGeneration = entity.Metadata.Generation;
        entity.Status.Synced = false;
        entity.Status.ErrorMessage = exception.Message;
        await client.UpdateStatusAsync(entity, cancellationToken);
        
        entityRequeue(entity, TimeSpan.FromSeconds(200));
    }

    private async Task HandleSuccessResult(Group entity, IdPGroup group, CancellationToken cancellationToken)
    {
        entity.Status.ObservedGeneration = entity.Metadata.Generation;
        entity.Status.DisplayName = group.Status.DisplayName;
        entity.Status.GroupId = group.Status.GroupId;
        entity.Status.ObservedGeneration = entity.Metadata.Generation;
        entity.Status.MemberOfGroupIds = group.Status.MemberOfGroupIds;
        entity.Status.Synced = true;
        entity.Status.ErrorMessage = string.Empty;

        await client.UpdateStatusAsync(entity, cancellationToken);
        
        entityRequeue(entity, TimeSpan.FromSeconds(20));
    }
}