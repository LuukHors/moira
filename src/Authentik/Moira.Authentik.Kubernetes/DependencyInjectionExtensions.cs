using FluentValidation;
using KubeOps.Abstractions.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moira.Authentik.Application.Models;
using Moira.Authentik.Controllers;
using Moira.Authentik.Kubernetes.Controllers;
using Moira.Authentik.Kubernetes.Group;
using Moira.Authentik.Kubernetes.OidcApplication;
using Moira.Authentik.Kubernetes.Provider;
using Moira.Authentik.Kubernetes.ValidatorWebhooks.Validators;
using Moira.Common.Abstractions.Models;
using Moira.Common.Kubernetes.AdapterHandler;
using Moira.Common.Kubernetes.DependencyProvider;
using Moira.Common.Kubernetes.PreReconcileSteps;
using Moira.Common.Kubernetes.PreReconcileSteps.ValidatorWebhooks.Executor;
using Moira.Common.Kubernetes.ResultHandler;
using Moira.Common.Kubernetes.Secrets;

namespace Moira.Authentik.Kubernetes;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMoiraAuthentik(this IServiceCollection services, bool enabled, IOperatorBuilder builder)
    {
        if (!enabled)
            return services;
        
        services.AddMoiraAuthentikProvider();

        services.AddScoped<IAdapterHandler<AuthentikGroup>, AdapterHandler<AuthentikGroup, AuthentikGroupModel>>();
        services.AddScoped<IAdapterHandler<AuthentikOidcApplication>, AdapterHandler<AuthentikOidcApplication, AuthentikOidcApplicationModel>>();
        services.AddScoped<IAdapterHandler<AuthentikProvider>, AdapterHandler<AuthentikProvider, IdPProvider>>();

        services.AddScoped<IResultHandler<AuthentikGroup, AuthentikGroupModel>, AuthentikGroupResultHandler>();
        services.AddScoped<IResultHandler<AuthentikOidcApplication, AuthentikOidcApplicationModel>, AuthentikOidcApplicationResultHandler>();
        services.AddScoped<IResultHandler<AuthentikProvider, IdPProvider>, AuthentikProviderResultHandler>();

        services.AddScoped<IPreReconcileSteps<AuthentikGroup>, AuthentikGroupPreReconcileSteps>();
        services.AddScoped<IPreReconcileSteps<AuthentikOidcApplication>, AuthentikOidcApplicationPreReconcileSteps>();
        services.AddScoped<IPreReconcileSteps<AuthentikProvider>, AuthentikProviderPreReconcileSteps>();

        services.AddScoped<IDependencyProvider<AuthentikGroup, AuthentikGroupModel>, AuthentikGroupDependencyProvider>();
        services.AddScoped<IDependencyProvider<AuthentikOidcApplication, AuthentikOidcApplicationModel>, AuthentikOidcApplicationDependencyProvider>();
        services.AddScoped<IDependencyProvider<AuthentikProvider, IdPProvider>, AuthentikProviderDependencyProvider>();

        services.AddScoped<IOidcApplicationSecretService<AuthentikOidcApplication>, OidcApplicationSecretService<AuthentikOidcApplication>>();

        services.AddScoped<IValidatorExecutor<AuthentikGroup>, ValidatorExecutor<AuthentikGroup>>();
        services.AddScoped<IValidatorExecutor<AuthentikOidcApplication>, ValidatorExecutor<AuthentikOidcApplication>>();
        services.AddScoped<IValidatorExecutor<AuthentikProvider>, ValidatorExecutor<AuthentikProvider>>();

        services.AddScoped<AbstractValidator<AuthentikGroup>, AuthentikGroupValidator>();
        services.AddScoped<AbstractValidator<AuthentikOidcApplication>, AuthentikOidcApplicationValidator>();
        services.AddScoped<AbstractValidator<AuthentikProvider>, AuthentikProviderValidator>();

        builder.AddController<GroupController, AuthentikGroup>();
        builder.AddFinalizer<GroupFinalizer, AuthentikGroup>("moira.operator/AuthentikGroupFinalizer");
        builder.AddController<ProviderController, AuthentikProvider>();
        builder.AddFinalizer<ProviderFinalizer, AuthentikProvider>("moira.operator/AuthentikProviderFinalizer");
        builder.AddController<OidcApplicationController, AuthentikOidcApplication>();
        builder.AddFinalizer<OidcApplicationFinalizer, AuthentikOidcApplication>("moira.operator/AuthentikOidcApplicationFinalizer");

        return services;
    }
}