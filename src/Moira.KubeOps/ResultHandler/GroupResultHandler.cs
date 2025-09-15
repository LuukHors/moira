using Microsoft.Extensions.Logging;
using Moira.Common.Commands;
using Moira.Common.Models;

namespace Moira.KubeOps.ResultHandler;

public class GroupResultHandler(
    ILogger<GroupResultHandler> logger) : IResultHandler<IdPGroup>
{
    public Task HandleAsync(IdPCommandResult<IdPGroup> result, CancellationToken cancellationToken)
    {
        logger.LogInformation("[{commandId}][{entityType}][{entityName}] GroupResultHandler handling the result man", result.Id, nameof(IdPGroup), result.Entity.Name);
        return Task.CompletedTask;
    }
}