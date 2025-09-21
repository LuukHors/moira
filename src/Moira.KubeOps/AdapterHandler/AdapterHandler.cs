using KubeOps.Abstractions.Entities;
using Microsoft.Extensions.Logging;
using Moira.Common.Commands;
using Moira.Common.Exceptions;
using Moira.Common.Models;
using Moira.Common.Provider;
using Moira.KubeOps.DependencyProvider;
using Moira.KubeOps.Mappers;
using Moira.KubeOps.ResultHandler;

namespace Moira.KubeOps.AdapterHandler;

public class AdapterHandler<TK8SEntity, TEntity>(
    IDependencyProvider<TK8SEntity, TEntity> dependencyProvider,
    IProviderRouter<TEntity> providerRouter,
    IResultHandler<TK8SEntity, TEntity> resultHandler,
    ILogger<AdapterHandler<TK8SEntity, TEntity>> logger) : IAdapterHandler<TK8SEntity> where TK8SEntity : CustomKubernetesEntity where TEntity : IdPEntity
{
    public async Task HandleAsync(TK8SEntity entity, CancellationToken cancellationToken)
    {
        try
        {
            var idPEntity = await dependencyProvider.ResolveAsync(entity, cancellationToken);
            var provider = await providerRouter.ResolveAsync(idPEntity.IdPProvider.Type, cancellationToken);

            var command = new IdPCommand<TEntity>(Guid.NewGuid(), idPEntity);

            logger.LogDebug(
                "[{commandId}][{entityType}][{entityName}] Sending command to provider {providerName}", command.Id,
                typeof(TEntity).Name, idPEntity.Name, provider.Name);

            var result = await provider.ExecuteAsync(command, cancellationToken);

            logger.LogDebug(
                "[{commandId}][{entityType}][{entityName}] Received result from provider {providerName}, handling result..",
                command.Id, typeof(TEntity).Name, idPEntity.Name, provider.Name);

            await resultHandler.HandleAsync(entity, cancellationToken, result.Entity, result.Exception);
        }
        catch (InvalidOperationException ex)
        {
            var idpEx = ex.ToIdPException();
            await resultHandler.HandleAsync(entity, cancellationToken, null, idpEx);
        }
        catch (Exception ex)
        {
            var idpEx = ex.ToIdPException();
            await resultHandler.HandleAsync(entity, cancellationToken, null, idpEx);
            logger.LogError(ex, "");
        }
    }
}