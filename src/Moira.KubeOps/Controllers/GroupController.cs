using KubeOps.Abstractions.Controller;
using Microsoft.Extensions.Logging;
using Moira.Common.Models;
using Moira.Common.Provider;
using Moira.KubeOps.Entities;

namespace Moira.KubeOps.Controllers;

public class GroupController(
    IProviderRouter<IdPGroup> providerRouter,
    ILogger<GroupController> logger) : IEntityController<Group>
{
    public async Task ReconcileAsync(Group entity, CancellationToken cancellationToken)
    {
        try
        {
            var provider = await providerRouter.ResolveProviderAsync(entity.Spec.ProviderRef.Name, cancellationToken);
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