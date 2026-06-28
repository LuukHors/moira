using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Moira.Common.Models;
using Moira.KubeOps.AdapterHandler;
using Moira.KubeOps.AdapterHandler.DependencyProvider;
using Moira.KubeOps.AdapterHandler.DependencyProvider.GroupProviderSettings;
using Moira.KubeOps.AdapterHandler.DependencyProvider.OidcProviderSettings;
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
        services.AddScoped<IAdapterHandler<Group>, AdapterHandler<Group, IdPGroup>>();
        services.AddScoped<IAdapterHandler<OidcApplication>, AdapterHandler<OidcApplication, IdPOidcApplication>>();
        services.AddScoped<IAdapterHandler<Provider>, AdapterHandler<Provider, IdPProvider>>();
        services.AddScoped<IResultHandler<Group, IdPGroup>, GroupResultHandler>();
        services.AddScoped<IResultHandler<OidcApplication, IdPOidcApplication>, OidcApplicationResultHandler>();
        services.AddScoped<IResultHandler<Provider, IdPProvider>, ProviderResultHandler>();
        services.AddScoped<IPreReconcileSteps<Group>, GroupPreReconcileSteps>();
        services.AddScoped<IPreReconcileSteps<OidcApplication>, OidcApplicationPreReconcileSteps>();
        services.AddScoped<IPreReconcileSteps<Provider>, ProviderPreReconcileSteps>();
        services.AddScoped<IDependencyProvider<Group, IdPGroup>, GroupDependencyProvider>();
        services.AddScoped<IDependencyProvider<OidcApplication, IdPOidcApplication>, OidcApplicationDependencyProvider>();
        services.AddScoped<IDependencyProvider<Provider, IdPProvider>, ProviderDependencyProvider>();
        services.AddScoped<ISecretService, SecretService>();
        services.AddScoped<IOidcApplicationSecretService, OidcApplicationSecretService>();
        services.AddScoped<IProviderSettingsResolver<OidcProviderSettings>, AuthentikOidcProviderSettingsResolver>();
        services.AddScoped<IProviderSettingsService<OidcApplication, OidcProviderSettings>, OidcProviderSettingsService>();
        services.AddScoped<IProviderSettingsResolver<GroupProviderSettings>, AuthentikGroupProviderSettingsResolver>();
        services.AddScoped<IProviderSettingsService<Group, GroupProviderSettings>, GroupProviderSettingsService>();
        services.AddScoped<IValidatorExecutor<AuthentikOidcApplicationSettings>, ValidatorExecutor<AuthentikOidcApplicationSettings>>();
        services.AddScoped<IValidatorExecutor<AuthentikGroupSettings>, ValidatorExecutor<AuthentikGroupSettings>>();
        services.AddScoped<IValidatorExecutor<Group>, ValidatorExecutor<Group>>();
        services.AddScoped<IValidatorExecutor<OidcApplication>, ValidatorExecutor<OidcApplication>>();
        services.AddScoped<IValidatorExecutor<Provider>, ValidatorExecutor<Provider>>();
        services.AddScoped<AbstractValidator<AuthentikOidcApplicationSettings>, AuthentikOidcApplicationSettingsValidator>();
        services.AddScoped<AbstractValidator<AuthentikGroupSettings>, AuthentikGroupSettingsValidator>();
        services.AddScoped<AbstractValidator<Group>, GroupValidator>();
        services.AddScoped<AbstractValidator<OidcApplication>, OIDCApplicationValidator>();
        services.AddScoped<AbstractValidator<Provider>, ProviderValidator>();
        return services;
    }
}
