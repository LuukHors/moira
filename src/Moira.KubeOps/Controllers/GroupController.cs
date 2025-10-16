using KubeOps.Abstractions.Controller;
using Microsoft.Extensions.Logging;
using Moira.KubeOps.AdapterHandler;
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
            await handler.HandleReconcileAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Something went wrong");
        }
    }

    public async Task DeletedAsync(Group entity, CancellationToken cancellationToken)
    {
        try
        {
            await handler.HandleDeleteAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Something went wrong");
        }
    }
}