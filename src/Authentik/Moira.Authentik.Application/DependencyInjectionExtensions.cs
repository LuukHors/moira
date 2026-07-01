using Moira.Authentik.Application.Models;
using Microsoft.Extensions.DependencyInjection;
using Moira.Authentik.Application.Builders;
using Moira.Authentik.Application.DefaultConfig;
using Moira.Authentik.Application.Handlers;
using Moira.Authentik.Application.UpdateCheckers;
using Moira.Authentik.Domain.Applications;
using Moira.Authentik.Domain.Groups;
using Moira.Authentik.Domain.ProviderSettings;
using Moira.Common;
using Moira.Common.Abstractions;
using Moira.Common.Abstractions.Models;

namespace Moira.Authentik.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddAuthentikApplication(this IServiceCollection services)
    {
        services.AddScoped<IUpdateChecker<AuthentikGroupV3, AuthentikGroupV3>, AuthentikGroupUpdateChecker>();
        services.AddScoped<IUpdateChecker<AuthentikOAuth2ProviderV3, AuthentikOAuth2ProviderV3>, AuthentikOAuth2ProviderUpdateChecker>();
        services.AddScoped<IUpdateChecker<AuthentikApplicationV3, AuthentikApplicationV3>, AuthentikApplicationUpdateChecker>();

        services.AddScoped<IAuthentikFlowBuilder, AuthentikFlowBuilder>();
        services.AddScoped<IAuthentikScopeMappingBuilder, AuthentikScopeMappingBuilder>();
        services.AddScoped<IAuthentikOAuth2ProviderBuilder, AuthentikOAuth2ProviderBuilder>();
        services.AddScoped<IAuthentikApplicationBuilder, AuthentikApplicationBuilder>();
        services.AddScoped<IAuthentikGroupBuilder, AuthentikGroupBuilder>();

        services.AddScoped<IAuthentikHandler<AuthentikGroupModel, AuthentikGroupV3>, AuthentikGroupHandler>();
        services.AddScoped<IAuthentikOidcApplicationHandler, AuthentikOidcApplicationHandler>();
        services.AddScoped<IAuthentikHandler<AuthentikOidcApplicationModel, AuthentikOidcApplicationV3>, AuthentikOidcApplicationHandler>();
        services.AddScoped<IAuthentikProviderHandler, AuthentikProviderHandler>();
        services.AddScoped<IDefaultConfig<OidcAuthentikProviderSettings>, AuthentikOidcApplicationSettingsDefault>();

        return services;
    }
}