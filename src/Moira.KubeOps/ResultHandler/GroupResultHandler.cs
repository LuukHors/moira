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
    public async Task HandleAsync(Group entity, IdPGroup idpEntity, CancellationToken cancellationToken)
    {
        entity.Status.ObservedGeneration = entity.Metadata.Generation;
        entity.Status.DisplayName = idpEntity.Status.DisplayName;
        entity.Status.GroupId = idpEntity.Status.GroupId;
        entity.Status.ObservedGeneration = entity.Metadata.Generation;
        entity.Status.MemberOfGroupIds = idpEntity.Status.MemberOfGroupIds;
        entity.Status.Synced = true;
        entity.Status.ErrorMessage = string.Empty;

        await client.UpdateStatusAsync(entity, cancellationToken);
        
        entityRequeue(entity, TimeSpan.FromSeconds(20));
    }

    public async Task HandleExceptionAsync(Group entity, IdPException exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "");
        
        entity.Status.ObservedGeneration = entity.Metadata.Generation;
        entity.Status.Synced = false;
        entity.Status.ErrorMessage = exception.Message;
        await client.UpdateStatusAsync(entity, cancellationToken);
        
        entityRequeue(entity, TimeSpan.FromSeconds(20));
    }
}