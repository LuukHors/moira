using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Moira.Common.Models;
using Moira.KubeOps.AdapterHandler;
using Moira.KubeOps.AdapterHandler.DependencyProvider;
using Moira.KubeOps.Entities;
using Moira.KubeOps.Entities.Validators;
using Moira.KubeOps.PreReconcileSteps;
using Moira.KubeOps.ResultHandler;
using Moira.KubeOps.PreReconcileSteps.ValidatorWebhooks.Executor;
using Moira.KubeOps.Secrets;

namespace Moira.KubeOps;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMoiraKubeOps(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddScoped<IAdapterHandler<Group>, AdapterHandler<Group, IdPGroup>>();
        services.AddScoped<IAdapterHandler<OidcApplication>, AdapterHandler<OidcApplication, IdPOidcApplication>>();
        services.AddScoped<IAdapterHandler<Provider>, AdapterHandler<Provider, IdPProvider>>();
        services.AddScoped<ISecretService, SecretService>();
        services.AddScoped<IOidcApplicationSecretService, OidcApplicationSecretService>();
        services.AddScoped<AbstractValidator<AuthentikOidcApplicationSettings>, AuthentikOidcApplicationSettingsValidator>();
        services.AddScoped<AbstractValidator<Group>, GroupValidator>();
        services.AddScoped<AbstractValidator<OidcApplication>, OIDCApplicationValidator>();
        services.AddScoped<AbstractValidator<Provider>, ProviderValidator>();
        return services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IResultHandler<,>)))
            .AsImplementedInterfaces()
            .AddClasses(classes => classes.AssignableTo(typeof(IPreReconcileSteps<>)))
            .AsImplementedInterfaces()
            .AddClasses(classes => classes.AssignableTo(typeof(IValidatorExecutor<>)))
            .AsSelfWithInterfaces()
            .AddClasses(c => c.AssignableTo(typeof(IDependencyProvider<,>)))
            .AsSelfWithInterfaces()
            .WithScopedLifetime());
    }
}
