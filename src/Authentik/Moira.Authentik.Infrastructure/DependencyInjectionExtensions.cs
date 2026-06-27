using Microsoft.Extensions.DependencyInjection;
using Moira.Authentik.Application.Ports;
using Moira.Authentik.Domain.Applications;
using Moira.Authentik.Domain.Groups;
using Moira.Authentik.Infrastructure.Authentication;
using Moira.Authentik.Infrastructure.Http;
using Moira.Authentik.Infrastructure.Http.Routes;
using Moira.Authentik.Infrastructure.Http.Routes.V3;
using Moira.Authentik.Infrastructure.ProviderCheck;
using Moira.Authentik.Infrastructure.Version;

namespace Moira.Authentik.Infrastructure;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddAuthentikInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IAuthentikAuthenticationService, AuthentikAuthenticationService>();
        services.AddScoped<IAuthentikProviderCheckService, AuthentikProviderCheckService>();
        services.AddScoped<IAuthentikVersionService, AuthentikVersionService>();
        services.AddScoped(typeof(IAuthentikRepository<,,>), typeof(AuthentikHttpService<,,>));

        services.AddScoped<IAuthentikRoute<AuthentikGroupV3, AuthentikGroupV3, string>, GroupRouteV3>();
        services.AddScoped<IAuthentikRoute<AuthentikApplicationV3, AuthentikApplicationV3, string>, ApplicationRouteV3>();
        services.AddScoped<IAuthentikRoute<AuthentikOAuth2ProviderV3, AuthentikOAuth2ProviderV3, int>, OAuth2ProviderRouteV3>();
        services.AddScoped<IAuthentikRoute<AuthentikFlowV3, AuthentikFlowV3, string>, FlowRouteV3>();
        services.AddScoped<IAuthentikRoute<AuthentikScopeMappingV3, AuthentikScopeMappingV3, string>, ScopeMappingRouteV3>();

        return services;
    }
}
