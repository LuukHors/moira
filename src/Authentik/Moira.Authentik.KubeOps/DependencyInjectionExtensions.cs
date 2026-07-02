using FluentValidation;
using KubeOps.Abstractions.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moira.Authentik.Application.Models;
using Moira.Authentik.Controllers;
using Moira.Authentik.KubeOps.Controllers;
using Moira.Authentik.KubeOps.DependencyProvider;
using Moira.Authentik.KubeOps.Entities;
using Moira.Authentik.KubeOps.Entities.Validators;
using Moira.Authentik.KubeOps.PreReconcileSteps;
using Moira.Authentik.KubeOps.ResultHandler;
using Moira.Authentik.KubeOps.Secrets;
using Moira.Common.Abstractions.Models;
using Moira.Common.KubeOps.AdapterHandler;
using Moira.Common.KubeOps.DependencyProvider;
using Moira.Common.KubeOps.PreReconcileSteps;
using Moira.Common.KubeOps.PreReconcileSteps.ValidatorWebhooks.Executor;
using Moira.Common.KubeOps.ResultHandler;
using Moira.Common.KubeOps.Secrets;

namespace Moira.Authentik.KubeOps;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMoiraAuthentik(this IServiceCollection services, IOperatorBuilder builder)
    {
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

        services.AddScoped<ISecretService, SecretService>();
        services.AddScoped<IOidcApplicationSecretService, OidcApplicationSecretService>();

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