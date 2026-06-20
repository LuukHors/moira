using k8s.Models;
using KubeOps.Abstractions.Entities;
using Microsoft.Extensions.Logging;
using Moira.Common.Commands;
using Moira.Common.Exceptions;
using Moira.Common.Models;
using Moira.Common.Provider;
using Moira.KubeOps.AdapterHandler.DependencyProvider;
using Moira.KubeOps.PreReconcileSteps;
using Moira.KubeOps.ResultHandler;

namespace Moira.KubeOps.AdapterHandler;

public class AdapterHandler<TK8SEntity, TEntity>(
    IPreReconcileSteps<TK8SEntity> preReconcileSteps,
    IDependencyProvider<TK8SEntity, TEntity> dependencyProvider,
    IProviderRouter<TEntity> providerRouter,
    IResultHandler<TK8SEntity, TEntity> resultHandler,
    ILogger<AdapterHandler<TK8SEntity, TEntity>> logger) : IAdapterHandler<TK8SEntity> where TK8SEntity : CustomKubernetesEntity where TEntity : IdPEntityBase
{
    public async Task HandleReconcileAsync(TK8SEntity entity, CancellationToken cancellationToken)
    {
        var operationId = Guid.NewGuid();
        using var _ = logger.BeginScope(new Dictionary<string, object>
        {
            { "OperationId", operationId },
            { "OperationType", "reconcile" },
            { "EntityKind", typeof(TEntity).Name },
            { "EntityName", entity.Name() },
            { "EntityNamespace", entity.Namespace() }
        });
        
        try
        {
            logger.LogDebug("Starting reconcile loop");

            logger.LogDebug("Executing pre-reconcile steps");
            var entityModified = await preReconcileSteps.ExecuteAsync(entity, cancellationToken);
            logger.LogDebug("Executed pre-reconcile steps");
            
            logger.LogDebug("Determining if entity was modified");
            if (entityModified)
            {
                logger.LogDebug("Entity was modified");
                return;
            }
            logger.LogDebug("Entity was not modified");
            
            logger.LogDebug("Resolving dependencies");
            var idPEntity = await dependencyProvider.ResolveAsync(entity, cancellationToken);
            var provider = await providerRouter.ResolveAsync(GetProviderType(idPEntity), cancellationToken);
            logger.LogDebug("Resolved dependencies for provider {ProviderName}", provider.Name);
            
            var command = new IdPCommand<TEntity>(operationId, idPEntity);

            logger.LogDebug("Sending reconcile command to provider {ProviderName}", provider.Name);
            var result = await provider.ExecuteReconcileAsync(command, cancellationToken);
            logger.LogDebug("Received reconcile result from provider {ProviderName}", provider.Name);

            await resultHandler.HandleAsync(entity, result.Entity, cancellationToken);
            
            logger.LogDebug("Completed reconcile loop");
        }
        catch (MoiraException ex)
        {
            logger.LogError(ex, "Reconcile operation failed with reason {FailureReason}", ex.Reason);
            await resultHandler.HandleExceptionAsync(entity, ex, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected reconcile operation failed");
            await resultHandler.HandleExceptionAsync(entity, new UnknownMoiraException("Unexpected reconciliation error.", ex), cancellationToken);
        }
    }

    public async Task HandleDeleteAsync(TK8SEntity entity, CancellationToken cancellationToken)
    {
        var operationId = Guid.NewGuid();
        using var _ = logger.BeginScope(new Dictionary<string, object>
        {
            { "OperationId", operationId },
            { "OperationType", "delete" },
            { "EntityKind", typeof(TEntity).Name },
            { "EntityName", entity.Name() },
            { "EntityNamespace", entity.Namespace() }
        });

        try
        {
            logger.LogDebug("Starting delete");
            
            logger.LogDebug("Resolving dependencies");
            var idPEntity = await dependencyProvider.ResolveAsync(entity, cancellationToken);
            var provider = await providerRouter.ResolveAsync(GetProviderType(idPEntity), cancellationToken);
            logger.LogDebug("Resolved dependencies for provider {ProviderName}", provider.Name);

            var command = new IdPCommand<TEntity>(operationId, idPEntity);
            
            logger.LogDebug("Sending delete command to provider {ProviderName}", provider.Name);
            var entityDeleted = await provider.ExecuteDeleteAsync(command, cancellationToken);
            logger.LogDebug("Received delete result from provider {ProviderName}", provider.Name);

            await resultHandler.HandleDeleteAsync(entity, idPEntity, cancellationToken);

            if (entityDeleted) logger.LogInformation("Entity was deleted");
        }
        catch (MoiraException ex)
        {
            logger.LogError(ex, "Delete operation failed with reason {FailureReason}", ex.Reason);
            await resultHandler.HandleExceptionAsync(entity, ex, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected delete operation failed");
            await resultHandler.HandleExceptionAsync(entity, new UnknownMoiraException("Unexpected deletion error.", ex), cancellationToken);
        }
    }

    private static string GetProviderType(TEntity entity)
    {
        return entity switch
        {
            IdPEntity idPEntity => idPEntity.IdPProvider.Type,
            IdPProvider idPProvider => idPProvider.Type,
            _ => throw new UnsupportedProviderException(typeof(TEntity).Name)
        };
    }
}
