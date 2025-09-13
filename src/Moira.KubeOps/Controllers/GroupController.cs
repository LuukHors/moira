using KubeOps.Abstractions.Controller;
using Microsoft.Extensions.Logging;
using Moira.Common.Exceptions;
using Moira.Common.Models;
using Moira.Common.Provider;
using Moira.KubeOps.Entities;
using Moira.KubeOps.Mappers;

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

            var command = entity.ToCommand();

            var result = await provider.ExecuteAsync(command);
        }
        catch(IdPException ex)
        {
            
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