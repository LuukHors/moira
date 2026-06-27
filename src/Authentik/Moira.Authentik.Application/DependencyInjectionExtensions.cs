using Microsoft.Extensions.DependencyInjection;
using Moira.Authentik.Application.Builders;
using Moira.Authentik.Application.Handlers;
using Moira.Authentik.Application.UpdateCheckers;
using Moira.Authentik.Domain.Applications;
using Moira.Authentik.Domain.Groups;
using Moira.Common.Models;
using Moira.Common.Provider;

namespace Moira.Authentik.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddAuthentikApplication(this IServiceCollection services)
    {
        services.AddScoped<IUpdateChecker<AuthentikGroupV3, AuthentikGroupV3>, AuthentikGroupUpdateChecker>();
        services.AddScoped<IUpdateChecker<AuthentikOAuth2ProviderV3, AuthentikOAuth2ProviderV3>, AuthentikOAuth2ProviderUpdateChecker>();
        services.AddScoped<IUpdateChecker<AuthentikApplicationV3, AuthentikApplicationV3>, AuthentikApplicationUpdateChecker>();

        services.AddScoped<IAuthentikOAuth2ProviderBuilder, AuthentikOAuth2ProviderBuilder>();
        services.AddScoped<IAuthentikApplicationBuilder, AuthentikApplicationBuilder>();
        services.AddScoped<IAuthentikGroupBuilder, AuthentikGroupBuilder>();

        services.AddScoped<IAuthentikHandler<IdPGroup, AuthentikGroupV3>, AuthentikGroupHandler>();
        services.AddScoped<IAuthentikOidcApplicationHandler, AuthentikOidcApplicationHandler>();
        services.AddScoped<IAuthentikHandler<IdPOidcApplication, AuthentikOidcApplicationV3>, AuthentikOidcApplicationHandler>();

        return services;
    }
}