using FluentValidation;
using KubeOps.Abstractions.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moira.Authentik.Application.Models;
using Moira.Authentik.Application.Models.Group;
using Moira.Authentik.Application.Models.OidcApplication;
using Moira.Authentik.Application.Models.Role;
using Moira.Authentik.Controllers;
using Moira.Authentik.Kubernetes.Controllers;
using Moira.Authentik.Kubernetes.Group;
using Moira.Authentik.Kubernetes.OidcApplication;
using Moira.Authentik.Kubernetes.Provider;
using Moira.Authentik.Kubernetes.Role;
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
        if (!enabled) return services;
        
        services.AddMoiraAuthentikProvider();

        services.AddScoped<IAdapterHandler<AuthentikGroup>, AdapterHandler<AuthentikGroup, AuthentikGroupModel>>();
        services.AddScoped<IAdapterHandler<AuthentikOidcApplication>, AdapterHandler<AuthentikOidcApplication, AuthentikOidcApplicationModel>>();
        services.AddScoped<IAdapterHandler<AuthentikProvider>, AdapterHandler<AuthentikProvider, IdPProvider>>();
        services.AddScoped<IAdapterHandler<AuthentikRole>, AdapterHandler<AuthentikRole, AuthentikRoleModel>>();

        services.AddScoped<IResultHandler<AuthentikGroup, AuthentikGroupModel>, AuthentikGroupResultHandler>();
        services.AddScoped<IResultHandler<AuthentikOidcApplication, AuthentikOidcApplicationModel>, OidcApplicationResultHandler>();
        services.AddScoped<IResultHandler<AuthentikProvider, IdPProvider>, ProviderResultHandler>();
        services.AddScoped<IResultHandler<AuthentikRole, AuthentikRoleModel>, RoleResultHandler>();
        
        services.AddScoped<IPreReconcileSteps<AuthentikGroup>, AuthentikGroupPreReconcileSteps>();
        services.AddScoped<IPreReconcileSteps<AuthentikOidcApplication>, OidcApplicationPreReconcileSteps>();
        services.AddScoped<IPreReconcileSteps<AuthentikProvider>, ProviderPreReconcileSteps>();
        services.AddScoped<IPreReconcileSteps<AuthentikRole>, RolePreReconcileSteps>();

        services.AddScoped<IDependencyProvider<AuthentikGroup, AuthentikGroupModel>, AuthentikGroupDependencyProvider>();
        services.AddScoped<IDependencyProvider<AuthentikOidcApplication, AuthentikOidcApplicationModel>, OidcApplicationDependencyProvider>();
        services.AddScoped<IDependencyProvider<AuthentikProvider, IdPProvider>, ProviderDependencyProvider>();
        services.AddScoped<IDependencyProvider<AuthentikRole, AuthentikRoleModel>, RoleDependencyProvider>();

        services.AddScoped<IOidcApplicationSecretService<AuthentikOidcApplication>, OidcApplicationSecretService<AuthentikOidcApplication>>();

        services.AddScoped<IValidatorExecutor<AuthentikGroup>, ValidatorExecutor<AuthentikGroup>>();
        services.AddScoped<IValidatorExecutor<AuthentikOidcApplication>, ValidatorExecutor<AuthentikOidcApplication>>();
        services.AddScoped<IValidatorExecutor<AuthentikProvider>, ValidatorExecutor<AuthentikProvider>>();
        services.AddScoped<IValidatorExecutor<AuthentikRole>, ValidatorExecutor<AuthentikRole>>();

        services.AddScoped<AbstractValidator<AuthentikGroup>, AuthentikGroupValidator>();
        services.AddScoped<AbstractValidator<AuthentikOidcApplication>, AuthentikOidcApplicationValidator>();
        services.AddScoped<AbstractValidator<AuthentikProvider>, AuthentikProviderValidator>();
        services.AddScoped<AbstractValidator<AuthentikRole>, AuthentikRoleValidator>();

        builder.AddController<GroupController, AuthentikGroup>();
        builder.AddFinalizer<GroupFinalizer, AuthentikGroup>("moira.operator/AuthentikGroupFinalizer");
        builder.AddController<ProviderController, AuthentikProvider>();
        builder.AddFinalizer<ProviderFinalizer, AuthentikProvider>("moira.operator/AuthentikProviderFinalizer");
        builder.AddController<OidcApplicationController, AuthentikOidcApplication>();
        builder.AddFinalizer<OidcApplicationFinalizer, AuthentikOidcApplication>("moira.operator/AuthentikOidcApplicationFinalizer");
        builder.AddController<RoleController, AuthentikRole>();
        builder.AddFinalizer<RoleFinalizer, AuthentikRole>("moira.operator/AuthentikRoleFinalizer");
        
        return services;
    }
}