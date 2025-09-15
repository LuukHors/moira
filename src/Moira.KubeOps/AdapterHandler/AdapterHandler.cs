using KubeOps.Abstractions.Entities;
using Microsoft.Extensions.Logging;
using Moira.Common.Commands;
using Moira.Common.Models;
using Moira.Common.Provider;
using Moira.KubeOps.Controllers;
using Moira.KubeOps.DependencyProvider;
using Moira.KubeOps.ResultHandler;

namespace Moira.KubeOps.AdapterHandler;

public class AdapterHandler<TK8SEntity, TEntity>(
    ISample sample,
    IDependencyProvider<TK8SEntity, TEntity> dependencyProvider,
    IProviderRouter<TEntity> providerRouter,
    IResultHandler<TEntity> resultHandler,
    ILogger<AdapterHandler<TK8SEntity, TEntity>> logger) : IAdapterHandler<TK8SEntity> where TK8SEntity : CustomKubernetesEntity where TEntity : IdPEntity
{
    public async Task HandleAsync(TK8SEntity entity, CancellationToken cancellationToken)
    {
        try
        {
            await sample.Test();
            var idPEntity = await dependencyProvider.ResolveAsync(entity, cancellationToken);
            var provider = await providerRouter.ResolveAsync(idPEntity.IdPProvider.Type, cancellationToken);

            var command = new IdPCommand<TEntity>(Guid.NewGuid(), idPEntity);
            
            logger.LogInformation("[{commandId}][{entityName}] Sending command to provider {providerName}", command.Id.ToString(), idPEntity.Name, provider.Name);

            var result = await provider.ExecuteAsync(command);

            logger.LogInformation("[{commandId}][{entityName}] Received result from provider {providerName}, handling result..", command.Id.ToString(), idPEntity.Name, provider.Name);

            await resultHandler.HandleAsync(result, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Something went wrong");
        }
    }
}