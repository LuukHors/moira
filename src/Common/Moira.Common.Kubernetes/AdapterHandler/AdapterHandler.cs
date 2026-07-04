using System.Diagnostics;
using k8s.Models;
using KubeOps.Abstractions.Entities;
using Microsoft.Extensions.Logging;
using Moira.Common.Abstractions;
using Moira.Common.Abstractions.Commands;
using Moira.Common.Abstractions.Exceptions;
using Moira.Common.Abstractions.Models;
using Moira.Common.Kubernetes.DependencyProvider;
using Moira.Common.Kubernetes.PreReconcileSteps;
using Moira.Common.Kubernetes.ResultHandler;

namespace Moira.Common.Kubernetes.AdapterHandler;

public class AdapterHandler<TK8SEntity, TEntity>(
    IPreReconcileSteps<TK8SEntity> preReconcileSteps,
    IDependencyProvider<TK8SEntity, TEntity> dependencyProvider,
    IProviderAdapter<TEntity> providerAdapter,
    IResultHandler<TK8SEntity, TEntity> resultHandler,
    ILogger<AdapterHandler<TK8SEntity, TEntity>> logger) : IAdapterHandler<TK8SEntity> where TK8SEntity : CustomKubernetesEntity where TEntity : IdPEntityBase
{
    public async Task HandleReconcileAsync(TK8SEntity entity, CancellationToken cancellationToken)
    {
        var timer = new Stopwatch();
        timer.Start();
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
            var entityModified = await preReconcileSteps.ExecuteAsync(entity, cancellationToken);
            if (entityModified)
                return;
            
            var idPEntity = await dependencyProvider.ResolveAsync(entity, cancellationToken);
            
            var command = new IdPCommand<TEntity>(operationId, idPEntity);
            var reconcileResult = await providerAdapter.ExecuteReconcileAsync(command, cancellationToken);

            await resultHandler.HandleReconcileResultAsync(entity, reconcileResult.Entity, cancellationToken);
            
            timer.Stop();
            logger.LogInformation("Finished reconcile loop in {Duration}ms", timer.ElapsedMilliseconds);
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
        var timer = new Stopwatch();
        timer.Start();
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
            var idPEntity = await dependencyProvider.ResolveAsync(entity, cancellationToken);

            var command = new IdPCommand<TEntity>(operationId, idPEntity);
            var entityDeleted = await providerAdapter.ExecuteDeleteAsync(command, cancellationToken);

            await resultHandler.HandleDeletedAsync(entity, idPEntity, cancellationToken);

            if (entityDeleted) logger.LogInformation("Entity was deleted");
            timer.Stop();
            logger.LogInformation("Finished reconcile loop in {Duration}ms", timer.ElapsedMilliseconds);
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
}
