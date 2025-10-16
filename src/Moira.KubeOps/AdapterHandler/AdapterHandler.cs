using k8s.Models;
using KubeOps.Abstractions.Entities;
using Microsoft.Extensions.Logging;
using Moira.Common.Commands;
using Moira.Common.Exceptions;
using Moira.Common.Models;
using Moira.Common.Provider;
using Moira.Common.RequestContext;
using Moira.KubeOps.DependencyProvider;
using Moira.KubeOps.Mappers;
using Moira.KubeOps.ResultHandler;

namespace Moira.KubeOps.AdapterHandler;

public class AdapterHandler<TK8SEntity, TEntity>(
    IDependencyProvider<TK8SEntity, TEntity> dependencyProvider,
    IProviderRouter<TEntity> providerRouter,
    IResultHandler<TK8SEntity, TEntity> resultHandler,
    IRequestContextProvider requestContext,
    ILogger<AdapterHandler<TK8SEntity, TEntity>> logger) : IAdapterHandler<TK8SEntity> where TK8SEntity : CustomKubernetesEntity where TEntity : IdPEntity
{
    public async Task HandleReconcileAsync(TK8SEntity entity, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogDebug("[{commandId}][{entityType}][{entityName}] Starting reconcile loop", requestContext.RequestId, typeof(TEntity).Name, entity.Name());
            
            logger.LogDebug("[{commandId}][{entityType}][{entityName}] Getting dependencies...", requestContext.RequestId, typeof(TEntity).Name, entity.Name());
            var idPEntity = await dependencyProvider.ResolveAsync(entity, cancellationToken);
            var provider = await providerRouter.ResolveAsync(idPEntity.IdPProvider.Type, cancellationToken);
            logger.LogDebug("[{commandId}][{entityType}][{entityName}] Received dependencies...", requestContext.RequestId, typeof(TEntity).Name, entity.Name());
            
            var command = new IdPCommand<TEntity>(requestContext.RequestId, idPEntity);

            logger.LogDebug(
                "[{commandId}][{entityType}][{entityName}] Sending command to provider {providerName}", requestContext.RequestId,
                typeof(TEntity).Name, idPEntity.Name, provider.Name);

            var result = await provider.ExecuteReconcileAsync(command, cancellationToken);

            logger.LogDebug(
                "[{commandId}][{entityType}][{entityName}] Received result from provider {providerName}, handling result..",
                requestContext.RequestId, typeof(TEntity).Name, idPEntity.Name, provider.Name);

            await resultHandler.HandleAsync(entity, result.Entity, cancellationToken);
            
            logger.LogDebug("[{commandId}][{entityType}][{entityName}] Completed reconcile loop", requestContext.RequestId, typeof(TEntity).Name, entity.Name());
        }
        catch (InvalidOperationException ex)
        {
            var idpEx = ex.ToIdPException();
            await resultHandler.HandleExceptionAsync(entity, idpEx, cancellationToken);
        }
        catch (HttpException ex)
        {
            var idpEx = ex.ToIdPException();
            await resultHandler.HandleExceptionAsync(entity, idpEx, cancellationToken);
        }
        catch (Exception ex)
        {
            var idpEx = ex.ToIdPException();
            await resultHandler.HandleExceptionAsync(entity, idpEx, cancellationToken);
        }
    }

    public Task HandleDeleteAsync(TK8SEntity entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}