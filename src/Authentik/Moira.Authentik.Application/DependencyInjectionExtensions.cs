using Moira.Authentik.Application.Models;
using Microsoft.Extensions.DependencyInjection;
using Moira.Authentik.Application.Builders;
using Moira.Authentik.Application.Handlers;
using Moira.Authentik.Application.Models.Group;
using Moira.Authentik.Application.Models.Role;
using Moira.Authentik.Application.UpdateCheckers;
using Moira.Authentik.Domain.Applications;
using Moira.Authentik.Domain.Groups;
using Moira.Authentik.Domain.Roles;
using Moira.Common.Abstractions;

namespace Moira.Authentik.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddAuthentikApplication(this IServiceCollection services)
    {
        services.AddScoped<IUpdateChecker<AuthentikGroupV3, AuthentikGroupV3>, AuthentikGroupUpdateChecker>();
        services.AddScoped<IUpdateChecker<AuthentikOAuth2ProviderV3, AuthentikOAuth2ProviderV3>, AuthentikOAuth2ProviderUpdateChecker>();
        services.AddScoped<IUpdateChecker<AuthentikApplicationV3, AuthentikApplicationV3>, AuthentikApplicationUpdateChecker>();
        services.AddScoped<IUpdateChecker<AuthentikRoleV3, AuthentikRoleV3>, AuthentikRoleUpdateChecker>();
        
        services.AddScoped<IAuthentikFlowBuilder, AuthentikFlowBuilder>();
        services.AddScoped<IAuthentikScopeMappingBuilder, AuthentikScopeMappingBuilder>();
        services.AddScoped<IAuthentikOAuth2ProviderBuilder, AuthentikOAuth2ProviderBuilder>();
        services.AddScoped<IAuthentikApplicationBuilder, AuthentikApplicationBuilder>();
        services.AddScoped<IAuthentikGroupBuilder, AuthentikGroupBuilder>();

        services.AddScoped<IAuthentikHandler<AuthentikGroupModel, AuthentikGroupV3>, AuthentikGroupHandler>();
        services.AddScoped<IAuthentikOidcApplicationHandler, AuthentikOidcApplicationHandler>();
        services.AddScoped<IAuthentikHandler<AuthentikRoleModel, AuthentikRoleV3>, AuthentikRoleApplicationHandler>();
        services.AddScoped<IAuthentikProviderHandler, AuthentikProviderHandler>();

        return services;
    }
}