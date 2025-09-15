using KubeOps.Abstractions.Controller;
using Microsoft.Extensions.Logging;
using Moira.Common.Exceptions;
using Moira.Common.Models;
using Moira.KubeOps.AdapterHandler;
using Moira.KubeOps.DependencyProvider;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.Controllers;

public class GroupController(
    IAdapterHandler<Group> handler,
    ILogger<GroupController> logger) : IEntityController<Group>
{
    public async Task ReconcileAsync(Group entity, CancellationToken cancellationToken)
    {
        try
        {
            await handler.HandleAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Something went wrong");
        }
    }

    public Task DeletedAsync(Group entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}