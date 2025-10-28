using k8s.Models;
using KubeOps.Abstractions.Entities;
using Microsoft.Extensions.Logging;
using Moira.Common.Commands;
using Moira.Common.Exceptions;
using Moira.Common.Models;
using Moira.Common.Provider;
using Moira.KubeOps.DependencyProvider;
using Moira.KubeOps.Mappers;
using Moira.KubeOps.PreReconcileSteps;
using Moira.KubeOps.ResultHandler;

namespace Moira.KubeOps.AdapterHandler;

public class AdapterHandler<TK8SEntity, TEntity>(
    IPreReconcileSteps<TK8SEntity> preReconcileSteps,
    IDependencyProvider<TK8SEntity, TEntity> dependencyProvider,
    IProviderRouter<TEntity> providerRouter,
    IResultHandler<TK8SEntity, TEntity> resultHandler,
    ILogger<AdapterHandler<TK8SEntity, TEntity>> logger) : IAdapterHandler<TK8SEntity> where TK8SEntity : CustomKubernetesEntity where TEntity : IdPEntity
{
    public async Task HandleReconcileAsync(TK8SEntity entity, CancellationToken cancellationToken)
    {
        var requestId = Guid.NewGuid();
        using var _ = logger.BeginScope(new Dictionary<string, object>
        {
            { "RequestId", requestId },
            { "EntityType", typeof(TEntity).Name },
            { "EntityName", entity.Name() }
        });
        
        try
        {
            logger.LogDebug("Starting reconcile loop");

            logger.LogDebug("Executing pre-concile steps loop");
            var entityModified = await preReconcileSteps.ExecuteAsync(entity, cancellationToken);
            logger.LogDebug("Executed pre-concile steps loop");
            
            logger.LogDebug("Determining if entity was modified");
            if (entityModified)
            {
                logger.LogDebug("Entity was modified");
                return;
            }
            logger.LogDebug("Entity was not");
            
            logger.LogDebug("Getting dependencies...");
            var idPEntity = await dependencyProvider.ResolveAsync(entity, cancellationToken);
            var provider = await providerRouter.ResolveAsync(idPEntity.IdPProvider.Type, cancellationToken);
            logger.LogDebug("Received dependencies...");
            
            var command = new IdPCommand<TEntity>(requestId, idPEntity);

            logger.LogDebug("Sending reconcile command to provider {providerName}", provider.Name);
            var result = await provider.ExecuteReconcileAsync(command, cancellationToken);
            logger.LogDebug("Received result from provider {providerName}, handling result..", provider.Name);

            await resultHandler.HandleAsync(entity, result.Entity, cancellationToken);
            
            logger.LogDebug("Completed reconcile loop");
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

    public async Task HandleDeleteAsync(TK8SEntity entity, CancellationToken cancellationToken)
    {
        var requestId = Guid.NewGuid();
        using var _ = logger.BeginScope(new Dictionary<string, object>
        {
            { "RequestId", requestId },
            { "EntityType", typeof(TEntity).Name },
            { "EntityName", entity.Name() }
        });

        try
        {
            logger.LogDebug("Starting delete");
            
            logger.LogDebug("Getting dependencies...");
            var idPEntity = await dependencyProvider.ResolveAsync(entity, cancellationToken);
            var provider = await providerRouter.ResolveAsync(idPEntity.IdPProvider.Type, cancellationToken);
            logger.LogDebug("Received dependencies...");

            var command = new IdPCommand<TEntity>(requestId, idPEntity);
            
            logger.LogDebug("Sending delete command to provider {providerName}", provider.Name);
            await provider.ExecuteDeleteAsync(command, cancellationToken);
            logger.LogDebug("Got result from provider {providerName}", provider.Name);

            await resultHandler.HandleDeleteAsync(entity, idPEntity, cancellationToken);
            
            logger.LogInformation("Deleted entity");
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
}