using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Moira.Authentik.Application.Models;
using Moira.Authentik.Controllers;
using Moira.Authentik.KubeOps.AdapterHandler.DependencyProvider;
using Moira.Authentik.KubeOps.Entities;
using Moira.Authentik.KubeOps.Entities.Validators;
using Moira.Authentik.KubeOps.PreReconcileSteps;
using Moira.Authentik.KubeOps.ResultHandler;
using Moira.Authentik.KubeOps.Secrets;
using Moira.Common.Abstractions.Models;
using Moira.Common.KubeOps.AdapterHandler;
using Moira.Common.KubeOps.AdapterHandler.DependencyProvider;
using Moira.Common.KubeOps.PreReconcileSteps;
using Moira.Common.KubeOps.PreReconcileSteps.ValidatorWebhooks.Executor;
using Moira.Common.KubeOps.ResultHandler;
using Moira.Common.KubeOps.Secrets;

namespace Moira.Authentik.KubeOps;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMoiraAuthentikKubeOps(this IServiceCollection services)
    {
        services.AddMoiraAuthentikProvider();

        services.AddScoped<IAdapterHandler<AuthentikGroup>, AdapterHandler<AuthentikGroup, AuthentikGroupModel>>();
        services.AddScoped<IAdapterHandler<AuthentikOidcApplication>, AdapterHandler<AuthentikOidcApplication, AuthentikOidcApplicationModel>>();
        services.AddScoped<IAdapterHandler<AuthentikProvider>, AdapterHandler<AuthentikProvider, IdPProvider>>();

        services.AddScoped<IResultHandler<AuthentikGroup, AuthentikGroupModel>, GroupResultHandler>();
        services.AddScoped<IResultHandler<AuthentikOidcApplication, AuthentikOidcApplicationModel>, OidcApplicationResultHandler>();
        services.AddScoped<IResultHandler<AuthentikProvider, IdPProvider>, ProviderResultHandler>();

        services.AddScoped<IPreReconcileSteps<AuthentikGroup>, GroupPreReconcileSteps>();
        services.AddScoped<IPreReconcileSteps<AuthentikOidcApplication>, OidcApplicationPreReconcileSteps>();
        services.AddScoped<IPreReconcileSteps<AuthentikProvider>, ProviderPreReconcileSteps>();

        services.AddScoped<IDependencyProvider<AuthentikGroup, AuthentikGroupModel>, GroupDependencyProvider>();
        services.AddScoped<IDependencyProvider<AuthentikOidcApplication, AuthentikOidcApplicationModel>, OidcApplicationDependencyProvider>();
        services.AddScoped<IDependencyProvider<AuthentikProvider, IdPProvider>, ProviderDependencyProvider>();

        services.AddScoped<ISecretService, SecretService>();
        services.AddScoped<IOidcApplicationSecretService, OidcApplicationSecretService>();

        services.AddScoped<IValidatorExecutor<AuthentikGroup>, ValidatorExecutor<AuthentikGroup>>();
        services.AddScoped<IValidatorExecutor<AuthentikOidcApplication>, ValidatorExecutor<AuthentikOidcApplication>>();
        services.AddScoped<IValidatorExecutor<AuthentikProvider>, ValidatorExecutor<AuthentikProvider>>();

        services.AddScoped<AbstractValidator<AuthentikGroup>, GroupValidator>();
        services.AddScoped<AbstractValidator<AuthentikOidcApplication>, OIDCApplicationValidator>();
        services.AddScoped<AbstractValidator<AuthentikProvider>, ProviderValidator>();

        return services;
    }
}