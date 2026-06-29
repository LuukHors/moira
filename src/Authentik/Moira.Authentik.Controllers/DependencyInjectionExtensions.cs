using Microsoft.Extensions.DependencyInjection;
using Moira.Authentik.Application;
using Moira.Authentik.Controllers.Adapters;
using Moira.Authentik.Infrastructure;
using Moira.Authentik.KubeOps;
using Moira.Common.Models;
using Moira.Common.Provider;

namespace Moira.Authentik.Controllers;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMoiraAuthentikProvider(this IServiceCollection services)
    {
        services
            .AddAuthentikKubeOps()
            .AddAuthentikInfrastructure()
            .AddAuthentikApplication();

        services.AddScoped<IProviderAdapter<IdPGroup>, AuthentikGroupProviderAdapter>();
        services.AddScoped<IProviderAdapter<IdPOidcApplication>, AuthentikApplicationProviderAdapter>();
        services.AddScoped<IProviderAdapter<IdPProvider>, AuthentikProviderAdapter>();

        return services;
    }
}
