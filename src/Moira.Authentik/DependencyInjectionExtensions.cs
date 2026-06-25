using Microsoft.Extensions.DependencyInjection;
using Moira.Authentik.Authentication;
using Moira.Authentik.Handlers;
using Moira.Authentik.HttpService;
using Moira.Authentik.HttpService.Routes;
using Moira.Authentik.Models.V3;
using Moira.Authentik.ProviderAdapters;
using Moira.Authentik.ProviderCheck;
using Moira.Common.Models;
using Moira.Common.Provider;

namespace Moira.Authentik;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMoiraAuthentikProvider(this IServiceCollection services)
    {
        services.AddSingleton<IAuthentikAuthenticationService, AuthentikAuthenticationService>();
        services.AddScoped<IAuthentikProviderCheckService, AuthentikProviderCheckService>();
        services.AddScoped(typeof(IHttpService<,,>), typeof(AuthentikHttpService<,,>));
        services.AddScoped<IAuthentikRoute<AuthentikApplicationV3, AuthentikApplicationV3, string>, ApplicationRoute>();
        services.AddScoped<IAuthentikRoute<AuthentikFlowV3, AuthentikFlowV3, string>, FlowRoute>();
        services.AddScoped<IAuthentikRoute<AuthentikGroupV3, AuthentikGroupV3, string>, GroupRoute>();
        services.AddScoped<IAuthentikRoute<AuthentikOAuth2ProviderV3, AuthentikOAuth2ProviderV3, int>, OAuth2ProviderRoute>();
        services.AddScoped<IAuthentikRoute<AuthentikScopeMappingV3, AuthentikScopeMappingV3, string>, ScopeMappingRoute>();
        services.AddScoped<IAuthentikHandler<IdPGroup, AuthentikGroupV3>, AuthentikGroupHandler>();
        services.AddScoped<IAuthentikOidcApplicationHandler, AuthentikOidcApplicationHandler>();
        services.AddScoped<IAuthentikHandler<IdPOidcApplication, AuthentikOidcApplicationV3>, AuthentikOidcApplicationHandler>();
        services.AddScoped<IProviderAdapter<IdPGroup>, AuthentikGroupProviderAdapter>();
        services.AddScoped<IProviderAdapter<IdPOidcApplication>, AuthentikApplicationProviderAdapter>();
        services.AddScoped<IProviderAdapter<IdPProvider>, AuthentikProviderAdapter>();

        return services;
    }
}
