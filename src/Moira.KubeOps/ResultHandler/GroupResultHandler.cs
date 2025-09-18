using k8s.Models;
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
    public async Task HandleAsync(Group entity, IdPCommandResult<IdPGroup> result, CancellationToken cancellationToken)
    {
        entity.Status.ObservedGeneration = entity.Metadata.Generation;
        entity.Status.DisplayName = result.Entity.Status.DisplayName;
        entity.Status.GroupId = result.Entity.Status.GroupId;
        entity.Status.ObservedGeneration = entity.Metadata.Generation;
        entity.Status.Synced = result.Status == IdPCommandResultStatus.Success;

        await client.UpdateStatusAsync(entity, cancellationToken);
        
        entityRequeue(entity, TimeSpan.FromSeconds(20));
    }
}